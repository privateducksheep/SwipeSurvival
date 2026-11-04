#if SWIPE_CAPTURE
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using TMPro;
using Unity.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Development-only trailer recorder. Like LayoutCapture it is compiled only into
/// builds made with the SWIPE_CAPTURE define (see BuildPlayer.BuildMacOSLayoutCapture).
///
/// Run the capture build with:
///   "Swipe Survival Layout Capture.app/Contents/MacOS/..." -trailerDir /path/to/output
///
/// It plays a scripted run of the real game and saves every frame as a JPEG plus
/// the game's sound as audio.wav. The game clock runs at exactly 30 fps
/// (Time.captureFramerate), so animation and sound stay in sync however long each
/// frame takes to save. Join the frames and audio into a video afterwards.
/// </summary>
public class TrailerCapture : MonoBehaviour
{
    const int FrameRate = 30;
    static readonly Vector2Int VideoSize = new Vector2Int(1920, 1080);

    //Cards played in the trailer, as indexes into resourceManager.cards
    const int DeathCard = 0;
    const int ZombieCard = 3;
    const int BirdenCard = 4;
    const int DesperateMotherCard = 7;
    const int FeralDogCard = 8;
    const int ElderlyManCard = 12;

    static readonly Color TextColour = new Color(0.96f, 0.89f, 0.72f);

    string outputDir;
    bool recording;
    int frameCount;
    int channels;
    readonly List<float> audioSamples = new List<float>();

    Image fade;
    Image touch;
    TextMeshProUGUI caption;
    TextMeshProUGUI subtitle;
    Image subtitleBand;
    AudioSource music;
    MethodInfo swipe;
    MethodInfo updateDays;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Boot()
    {
        string dir = GetArgument("-trailerDir");
        if (string.IsNullOrEmpty(dir))
            return;

        GameObject host = new GameObject("TrailerCapture");
        DontDestroyOnLoad(host);
        host.AddComponent<TrailerCapture>().outputDir = dir;
    }

    IEnumerator Start()
    {
        Directory.CreateDirectory(outputDir);
        //Full volume whatever this build last saved (applied as each scene loads)
        PlayerPrefs.SetFloat(VolumeSettings.MusicVolumeKey, 1f);
        PlayerPrefs.SetFloat(VolumeSettings.SfxVolumeKey, 1f);
        const BindingFlags Private = BindingFlags.Instance | BindingFlags.NonPublic;
        swipe = typeof(GameManager).GetMethod("Swipe", Private);
        updateDays = typeof(GameManager).GetMethod("UpdateDaysSurvivedUI", Private);

        Screen.SetResolution(VideoSize.x, VideoSize.y, FullScreenMode.Windowed);
        float waited = 0f;
        while ((Screen.width != VideoSize.x || Screen.height != VideoSize.y) && waited < 3f)
        {
            waited += Time.unscaledDeltaTime;
            yield return null;
        }

        //Borrow the gameplay music and font, so one track plays over the whole trailer
        yield return SceneManager.LoadSceneAsync("Playable");
        GameManager game = FindFirstObjectByType<GameManager>();
        AudioSource sceneMusic = FindSceneMusic();
        music = gameObject.AddComponent<AudioSource>();
        music.clip = sceneMusic.clip;
        music.outputAudioMixerGroup = sceneMusic.outputAudioMixerGroup;
        music.volume = sceneMusic.volume;
        music.loop = true;
        BuildOverlay(game.characterDialogue.font);
        SetAlpha(fade, 1f);

        yield return LoadScene("MainMenu");
        //The Exit button only exists in the desktop builds; the trailer shows the web game
        Button exit = FindButton("Exit");
        if (exit != null)
            exit.gameObject.SetActive(false);

        Time.captureFramerate = FrameRate;
        channels = ChannelCount(AudioSettings.speakerMode);
        AudioRenderer.Start();
        music.Play();
        recording = true;
        StartCoroutine(Record());

        //1. Title screen, and a tap on Start
        yield return Tween(0.8f, t => SetAlpha(fade, 1f - t));
        yield return Wait(2.2f);
        Button start = FindButton("Start");
        if (start != null)
            yield return Tap(start.GetComponent<RectTransform>());
        yield return Tween(0.5f, t => SetAlpha(fade, t));

        //2. A run already ten days in
        yield return LoadScene("Playable");
        game = FindFirstObjectByType<GameManager>();
        Card[] cards = game.resourceManager.cards;
        game.cardSwipeCount = 55;
        game.daysSurvived = 11;
        updateDays.Invoke(game, null);

        yield return CaptionIn("EVERY CARD IS A CHOICE", fromBlack: true);
        game.LoadCard(cards[DesperateMotherCard]);
        yield return CaptionOut();

        //Weighs up both answers before choosing
        yield return Press(game);
        float side = SwipeDistance(game);
        yield return DragTo(game, -side, 0.55f);
        yield return Wait(0.8f);
        yield return DragTo(game, side, 0.75f);
        yield return Wait(0.7f);
        yield return DragTo(game, -side, 0.65f);
        yield return Wait(0.4f);
        Choose(game, false);  //Share some food: food -15, morale +10

        //Each choice deals a random card; the script swaps in its own straight away
        game.LoadCard(cards[ZombieCard]);
        yield return SwipeCard(game, true, 0.8f);   //Fight: ammo -30, morale +30

        yield return CaptionIn("EVERY CHOICE HAS A COST");
        game.LoadCard(cards[BirdenCard]);
        yield return CaptionOut();
        yield return SwipeCard(game, false, 0.8f);  //Let him in: food -20, water -20

        game.LoadCard(cards[ElderlyManCard]);
        yield return SwipeCard(game, true, 0.8f);   //Every man for himself

        yield return CaptionIn("HOW MANY DAYS\nCAN YOU LAST?");
        game.LoadCard(cards[FeralDogCard]);
        yield return CaptionOut();
        yield return SwipeCard(game, false, 0.8f);  //Feed it: food hits zero on day 12

        //3. The death card, then the death screen
        yield return WaitForFlip(game);
        yield return Wait(0.5f);
        yield return Press(game);
        yield return DragTo(game, -side, 0.6f);
        yield return Wait(0.8f);
        yield return Tween(0.35f, t => SetAlpha(fade, t));
        Choose(game, false);
        while (SceneManager.GetActiveScene().name != "DeathScene")
            yield return null;
        yield return null;
        PrepareScene();
        yield return Tween(0.5f, t => SetAlpha(fade, 1f - t));
        yield return Wait(3f);
        yield return Tween(0.6f, t => SetAlpha(fade, t));

        //4. End card: the title screen without its buttons
        yield return LoadScene("MainMenu");
        foreach (Button button in FindObjectsByType<Button>(FindObjectsSortMode.None))
            button.gameObject.SetActive(false);
        subtitle.gameObject.SetActive(true);
        subtitleBand.gameObject.SetActive(true);
        yield return Tween(0.8f, t => { SetAlpha(fade, 1f - 0.75f * t); SetAlpha(subtitle, t); SetAlpha(subtitleBand, t); });
        yield return Wait(3.5f);
        float musicVolume = music.volume;
        yield return Tween(1.4f, t =>
        {
            SetAlpha(fade, 0.25f + 0.75f * t);
            SetAlpha(subtitle, 1f - t);
            SetAlpha(subtitleBand, 1f - t);
            music.volume = musicVolume * (1f - t);
        });
        yield return Wait(0.3f);

        recording = false;
        yield return null;
        yield return null;
        AudioRenderer.Stop();
        Time.captureFramerate = 0;

        int sampleRate = AudioSettings.outputSampleRate;
        WriteWav(Path.Combine(outputDir, "audio.wav"), audioSamples, channels, sampleRate);
        File.WriteAllLines(Path.Combine(outputDir, "trailer-info.txt"), new[]
        {
            "frames " + frameCount,
            "fps " + FrameRate,
            "screen " + Screen.width + "x" + Screen.height,
            "video seconds " + (frameCount / (float)FrameRate).ToString("0.00"),
            "audio seconds " + (audioSamples.Count / (float)channels / sampleRate).ToString("0.00"),
            "sample rate " + sampleRate,
            "channels " + channels,
        });
        Application.Quit();
    }

    /// <summary>Saves every rendered frame and the sound produced during it.</summary>
    IEnumerator Record()
    {
        WaitForEndOfFrame endOfFrame = new WaitForEndOfFrame();
        while (recording)
        {
            yield return endOfFrame;
            Texture2D frame = ScreenCapture.CaptureScreenshotAsTexture();
            File.WriteAllBytes(Path.Combine(outputDir, $"frame_{frameCount:00000}.jpg"), frame.EncodeToJPG(95));
            Destroy(frame);
            frameCount++;

            int sampleFrames = AudioRenderer.GetSampleCountForCaptureFrame();
            using (NativeArray<float> buffer = new NativeArray<float>(sampleFrames * channels, Allocator.Temp))
            {
                AudioRenderer.Render(buffer);
                audioSamples.AddRange(buffer);
            }
        }
    }

    // ---- Playing the game ----

    /// <summary>Picks up the current card, drags it to one side and lets go.</summary>
    IEnumerator SwipeCard(GameManager game, bool right, float hold)
    {
        yield return Press(game);
        float side = SwipeDistance(game);
        yield return DragTo(game, right ? side : -side, 0.55f);
        yield return Wait(hold);
        Choose(game, right);
    }

    IEnumerator WaitForFlip(GameManager game)
    {
        while (game.isSubstituting)
            yield return null;
    }

    /// <summary>Waits for the card to finish flipping, then puts a finger on it.</summary>
    IEnumerator Press(GameManager game)
    {
        yield return WaitForFlip(game);
        yield return Wait(0.25f);
        game.enabled = false;  //The script moves the card from here until the choice is made
        PlaceCard(game, game.cardGameObject.transform.position.x);
        yield return Tween(0.15f, t =>
        {
            SetAlpha(touch, 0.55f * t);
            touch.rectTransform.localScale = Vector3.one * Mathf.Lerp(1.4f, 1f, t);
        });
    }

    IEnumerator DragTo(GameManager game, float x, float seconds)
    {
        float from = game.cardGameObject.transform.position.x;
        yield return Tween(seconds, t => PlaceCard(game, Mathf.Lerp(from, x, t)));
    }

    /// <summary>Lets go of the card, which makes the choice on that side.</summary>
    void Choose(GameManager game, bool right)
    {
        SetAlpha(touch, 0f);
        swipe.Invoke(game, new object[] { right });
        game.enabled = true;
    }

    /// <summary>Moves the card as GameManager does while it is dragged, with the finger marker under it.</summary>
    void PlaceCard(GameManager game, float x)
    {
        Transform card = game.cardGameObject.transform;
        card.position = new Vector3(x, 1f - 0.3f * Mathf.Abs(x), card.position.z);
        card.eulerAngles = new Vector3(0f, 0f, x * game.fRotationCoefficient);

        float swipeScale = SwipeScale(game);
        Color color = game.textColor;
        color.a = Mathf.Clamp01((Mathf.Abs(x) - game.fSideMargin * swipeScale) / (game.divideValue * swipeScale));
        game.actionQuote.color = color;
        game.actionQuote.text = x < 0f ? game.currentCard.leftQuote : game.currentCard.rightQuote;

        touch.rectTransform.position = Camera.main.WorldToScreenPoint(card.position);
    }

    //Same as GameManager.Update
    static float SwipeScale(GameManager game)
    {
        float viewHalfWidth = Camera.main.orthographicSize * Camera.main.aspect;
        return Mathf.Min(1f, viewHalfWidth * 0.45f / game.fSideTrigger);
    }

    static float SwipeDistance(GameManager game)
    {
        return game.fSideTrigger * SwipeScale(game) + 0.4f;
    }

    IEnumerator Tap(RectTransform target)
    {
        touch.rectTransform.position = target.TransformPoint(target.rect.center);
        yield return Tween(0.15f, t =>
        {
            SetAlpha(touch, 0.55f * t);
            touch.rectTransform.localScale = Vector3.one * Mathf.Lerp(1.4f, 1f, t);
        });
        yield return Wait(0.25f);
        yield return Tween(0.2f, t => SetAlpha(touch, 0.55f * (1f - t)));
    }

    // ---- Overlay: fades, captions and the finger marker ----

    IEnumerator CaptionIn(string text, bool fromBlack = false)
    {
        caption.text = text;
        yield return Tween(0.35f, t =>
        {
            if (!fromBlack)
                SetAlpha(fade, 0.82f * t);
            SetAlpha(caption, t);
        });
        yield return Wait(1.3f);
    }

    IEnumerator CaptionOut()
    {
        float from = fade.color.a;
        yield return Tween(0.4f, t =>
        {
            SetAlpha(fade, from * (1f - t));
            SetAlpha(caption, 1f - t);
        });
    }

    void BuildOverlay(TMP_FontAsset font)
    {
        GameObject canvasObject = new GameObject("TrailerOverlay");
        canvasObject.transform.SetParent(transform);
        Canvas canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 1000;
        CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = VideoSize;
        scaler.matchWidthOrHeight = 1f;

        fade = NewImage(canvasObject.transform, "Fade", null, Color.black);
        RectTransform fadeRect = fade.rectTransform;
        fadeRect.anchorMin = Vector2.zero;
        fadeRect.anchorMax = Vector2.one;
        fadeRect.offsetMin = fadeRect.offsetMax = Vector2.zero;

        caption = NewText(canvasObject.transform, "Caption", font, 104f, new Vector2(0f, 0f));

        //Soft dark band so the end-card line reads over the busy title screen
        subtitleBand = NewImage(canvasObject.transform, "SubtitleBand", BandSprite(), Color.black);
        subtitleBand.rectTransform.anchorMin = new Vector2(0f, 0.5f);
        subtitleBand.rectTransform.anchorMax = new Vector2(1f, 0.5f);
        subtitleBand.rectTransform.sizeDelta = new Vector2(0f, 300f);
        subtitleBand.rectTransform.anchoredPosition = new Vector2(0f, -150f);
        SetAlpha(subtitleBand, 0f);
        subtitleBand.gameObject.SetActive(false);

        subtitle = NewText(canvasObject.transform, "Subtitle", font, 80f, new Vector2(0f, -150f));
        subtitle.text = "PLAY FREE IN YOUR BROWSER\n<size=60%>ON DESKTOP AND MOBILE</size>";
        subtitle.gameObject.SetActive(false);

        touch = NewImage(canvasObject.transform, "Touch", FingerSprite(), Color.white);
        touch.rectTransform.sizeDelta = new Vector2(96f, 96f);
        SetAlpha(touch, 0f);
    }

    static Image NewImage(Transform parent, string name, Sprite sprite, Color color)
    {
        GameObject imageObject = new GameObject(name, typeof(RectTransform));
        imageObject.transform.SetParent(parent, false);
        Image image = imageObject.AddComponent<Image>();
        image.sprite = sprite;
        image.color = color;
        image.raycastTarget = false;
        return image;
    }

    static TextMeshProUGUI NewText(Transform parent, string name, TMP_FontAsset font, float size, Vector2 position)
    {
        GameObject textObject = new GameObject(name, typeof(RectTransform));
        textObject.transform.SetParent(parent, false);
        TextMeshProUGUI text = textObject.AddComponent<TextMeshProUGUI>();
        text.font = font;
        text.fontSize = size;
        text.alignment = TextAlignmentOptions.Center;
        text.color = TextColour;
        text.characterSpacing = 4f;
        text.outlineWidth = 0.25f;
        text.outlineColor = Color.black;
        //Thickens the dot-matrix font so it reads at a glance
        text.fontMaterial.SetFloat(ShaderUtilities.ID_FaceDilate, 0.25f);
        text.raycastTarget = false;
        text.rectTransform.sizeDelta = new Vector2(1840f, 300f);
        text.rectTransform.anchoredPosition = position;
        SetAlpha(text, 0f);
        return text;
    }

    /// <summary>A soft white dot with a ring, standing in for the player's finger.</summary>
    static Sprite FingerSprite()
    {
        const int Size = 128;
        Texture2D texture = new Texture2D(Size, Size, TextureFormat.RGBA32, false);
        for (int y = 0; y < Size; y++)
        {
            for (int x = 0; x < Size; x++)
            {
                float r = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), new Vector2(Size / 2f, Size / 2f)) / (Size / 2f);
                float fill = Mathf.Clamp01((0.72f - r) / 0.04f) * 0.75f;
                float ring = Mathf.Clamp01(1f - Mathf.Abs(r - 0.86f) / 0.06f);
                texture.SetPixel(x, y, new Color(1f, 1f, 1f, Mathf.Max(fill, ring)));
            }
        }
        texture.Apply();
        return Sprite.Create(texture, new Rect(0, 0, Size, Size), new Vector2(0.5f, 0.5f));
    }

    /// <summary>Black that fades out towards the top and bottom edges.</summary>
    static Sprite BandSprite()
    {
        const int Height = 64;
        Texture2D texture = new Texture2D(1, Height, TextureFormat.RGBA32, false);
        texture.wrapMode = TextureWrapMode.Clamp;
        for (int y = 0; y < Height; y++)
        {
            float fromCentre = Mathf.Abs((y + 0.5f) / Height - 0.5f) * 2f;
            texture.SetPixel(0, y, new Color(1f, 1f, 1f, 0.65f * Mathf.SmoothStep(1f, 0f, fromCentre)));
        }
        texture.Apply();
        return Sprite.Create(texture, new Rect(0, 0, 1, Height), new Vector2(0.5f, 0.5f));
    }

    static void SetAlpha(Graphic graphic, float alpha)
    {
        Color color = graphic.color;
        color.a = alpha;
        graphic.color = color;
    }

    // ---- Timing ----

    /// <summary>Runs step(t) once per frame for the given time, with t easing from 0 to 1.</summary>
    static IEnumerator Tween(float seconds, System.Action<float> step)
    {
        int frames = Mathf.Max(1, Mathf.RoundToInt(seconds * FrameRate));
        for (int i = 1; i <= frames; i++)
        {
            step(Mathf.SmoothStep(0f, 1f, i / (float)frames));
            yield return null;
        }
    }

    static IEnumerator Wait(float seconds)
    {
        int frames = Mathf.RoundToInt(seconds * FrameRate);
        for (int i = 0; i < frames; i++)
            yield return null;
    }

    // ---- Scenes and sound ----

    IEnumerator LoadScene(string sceneName)
    {
        yield return SceneManager.LoadSceneAsync(sceneName);
        yield return null;  //Lets the scene's Start methods run
        PrepareScene();
    }

    /// <summary>
    /// One music track throughout, and no clicks or key presses from whoever is at
    /// the computer reaching the menus while the trailer records.
    /// </summary>
    void PrepareScene()
    {
        StopSceneMusic();
        foreach (EventSystem eventSystem in FindObjectsByType<EventSystem>(FindObjectsSortMode.None))
            eventSystem.enabled = false;
    }

    static AudioSource FindSceneMusic()
    {
        foreach (AudioSource source in FindObjectsByType<AudioSource>(FindObjectsSortMode.None))
        {
            if (source.clip != null && source.loop)
                return source;
        }
        return null;
    }

    /// <summary>Stops each scene's own music so the trailer keeps one track throughout.</summary>
    void StopSceneMusic()
    {
        foreach (AudioSource source in FindObjectsByType<AudioSource>(FindObjectsSortMode.None))
        {
            if (source != music && source.clip != null && source.loop)
                source.Stop();
        }
    }

    static Button FindButton(string name)
    {
        foreach (Button button in FindObjectsByType<Button>(FindObjectsSortMode.None))
        {
            if (button.name == name)
                return button;
        }
        return null;
    }

    static int ChannelCount(AudioSpeakerMode mode)
    {
        switch (mode)
        {
            case AudioSpeakerMode.Mono: return 1;
            case AudioSpeakerMode.Quad: return 4;
            case AudioSpeakerMode.Surround: return 5;
            case AudioSpeakerMode.Mode5point1: return 6;
            case AudioSpeakerMode.Mode7point1: return 8;
            default: return 2;
        }
    }

    static void WriteWav(string path, List<float> samples, int channelCount, int sampleRate)
    {
        using (BinaryWriter writer = new BinaryWriter(File.Create(path)))
        {
            int dataBytes = samples.Count * 2;
            writer.Write(Encoding.ASCII.GetBytes("RIFF"));
            writer.Write(36 + dataBytes);
            writer.Write(Encoding.ASCII.GetBytes("WAVEfmt "));
            writer.Write(16);
            writer.Write((short)1);
            writer.Write((short)channelCount);
            writer.Write(sampleRate);
            writer.Write(sampleRate * channelCount * 2);
            writer.Write((short)(channelCount * 2));
            writer.Write((short)16);
            writer.Write(Encoding.ASCII.GetBytes("data"));
            writer.Write(dataBytes);
            foreach (float sample in samples)
                writer.Write((short)Mathf.Clamp(Mathf.RoundToInt(sample * 32767f), -32768, 32767));
        }
    }

    static string GetArgument(string name)
    {
        string[] args = System.Environment.GetCommandLineArgs();
        for (int i = 0; i < args.Length - 1; i++)
        {
            if (args[i] == name)
                return args[i + 1];
        }
        return null;
    }
}
#endif
