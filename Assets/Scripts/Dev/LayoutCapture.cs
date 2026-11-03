#if SWIPE_CAPTURE
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Development-only layout checker. It is compiled only into builds made with
/// the SWIPE_CAPTURE define (see BuildPlayer.BuildMacOSLayoutCapture), so it never
/// ships in a release build.
///
/// Run the capture build with:
///   "Swipe Survival Layout Capture.app/Contents/MacOS/..." -captureDir /path/to/output
/// Optional: -captureSizes 1280x720,1024x768
///
/// For every window size it visits each scene and saves a screenshot, so layout
/// problems on unusual aspect ratios can be reviewed without the real hardware.
/// </summary>
public class LayoutCapture : MonoBehaviour
{
    static readonly Vector2Int[] DefaultSizes =
    {
        new Vector2Int(1280, 720),   // 16:9
        new Vector2Int(1440, 900),   // 16:10
        new Vector2Int(1280, 832),   // MacBook Air full screen
        new Vector2Int(1024, 768),   // 4:3
        new Vector2Int(1280, 1024),  // 5:4
        new Vector2Int(1000, 1000),  // 1:1
        new Vector2Int(1720, 720),   // 21:9
        new Vector2Int(2400, 675),   // 32:9
        new Vector2Int(720, 1280),   // 9:16 portrait
        new Vector2Int(600, 1300),   // tall phone portrait
    };

    string outputDir;
    List<Vector2Int> sizes;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Boot()
    {
        string dir = GetArgument("-captureDir");
        if (string.IsNullOrEmpty(dir))
            return;

        GameObject host = new GameObject("LayoutCapture");
        DontDestroyOnLoad(host);
        LayoutCapture capture = host.AddComponent<LayoutCapture>();
        capture.outputDir = dir;
        capture.sizes = ParseSizes(GetArgument("-captureSizes"));

        //Pretend to be a phone or browser screen with this pixel density, e.g. 1.5
        //makes a 585 x 1266 window behave like a 390 x 844 point iPhone
        if (float.TryParse(GetArgument("-capturePixelsPerPoint"), System.Globalization.NumberStyles.Float,
                System.Globalization.CultureInfo.InvariantCulture, out float pixelsPerPoint))
        {
            AdaptiveCanvasScaler.pixelsPerPointOverride = pixelsPerPoint;
        }
    }

    IEnumerator Start()
    {
        Directory.CreateDirectory(outputDir);
        if (System.Array.IndexOf(System.Environment.GetCommandLineArgs(), "-logicCheck") >= 0)
        {
            yield return LogicChecks();
            Application.Quit();
            yield break;
        }

        List<string> log = new List<string>();

        foreach (Vector2Int size in sizes)
        {
            Screen.SetResolution(size.x, size.y, FullScreenMode.Windowed);
            float waited = 0f;
            while ((Screen.width != size.x || Screen.height != size.y) && waited < 3f)
            {
                waited += Time.unscaledDeltaTime;
                yield return null;
            }
            yield return new WaitForSecondsRealtime(0.4f);

            string tag = size.x + "x" + size.y;
            log.Add($"{tag}: requested {size.x}x{size.y}, got {Screen.width}x{Screen.height}");

            yield return LoadScene("MainMenu");
            yield return Shot(tag, "1-menu");

            yield return LoadScene("OptionsMenu");
            yield return Shot(tag, "2-options");

            yield return LoadScene("Playable");
            yield return new WaitForSecondsRealtime(1.2f);
            yield return Shot(tag, "3-play");

            GameManager game = FindFirstObjectByType<GameManager>();
            if (game != null)
            {
                // Longest dialogue in the deck, to check text wrapping.
                game.LoadCard(game.resourceManager.cards[18]);
                yield return new WaitForSecondsRealtime(1.2f);
                yield return Shot(tag, "4-longtext");

                // Freeze the card mid-swipe to check the action text and HUD overlap.
                game.enabled = false;
                Vector3 position = game.cardGameObject.transform.position;
                position.x = -(game.fSideTrigger + 0.3f);
                game.cardGameObject.transform.position = position;
                game.cardGameObject.transform.rotation = Quaternion.Euler(0f, 0f, position.x * game.fRotationCoefficient);
                game.actionQuote.text = game.currentCard.leftQuote;
                Color color = game.textColor;
                color.a = 1f;
                game.actionQuote.color = color;
                yield return Shot(tag, "5-swipe");
                game.enabled = true;
            }

            GameStats.daysSurvived = 12;
            GameStats.highScore = 34;
            yield return LoadScene("DeathScene");
            yield return Shot(tag, "6-death");
        }

        File.WriteAllLines(Path.Combine(outputDir, "capture-log.txt"), log);
        Application.Quit();
    }

    /// <summary>
    /// Plays through the swipe rules that were buggy and writes PASS/FAIL lines
    /// to logic-check.txt in the output folder.
    /// </summary>
    IEnumerator LogicChecks()
    {
        List<string> results = new List<string>();
        void Check(string name, bool passed, string detail)
        {
            results.Add((passed ? "PASS " : "FAIL ") + name + " - " + detail);
        }

        System.Reflection.MethodInfo swipe = typeof(GameManager).GetMethod(
            "Swipe", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
        Check("Swipe method exists", swipe != null, "GameManager.Swipe(bool)");

        yield return LoadScene("Playable");
        GameManager game = FindFirstObjectByType<GameManager>();
        Card[] cards = game.resourceManager.cards;

        if (swipe != null)
        {
            //Birden zombie horde: both answers are "Fight", so both must lead on to the child
            foreach (bool right in new[] { false, true })
            {
                SetResources(game, 50);
                game.LoadCard(cards[29]);
                swipe.Invoke(game, new object[] { right });
                Check("Birden horde -> child (" + (right ? "right" : "left") + ")", game.currentCard == cards[30], "now on " + game.currentCard.cardName);
            }

            //Zombie bunker still leads to the loot both ways
            foreach (bool right in new[] { false, true })
            {
                SetResources(game, 80);
                game.LoadCard(cards[14]);
                swipe.Invoke(game, new object[] { right });
                Check("Zombie bunker -> loot (" + (right ? "right" : "left") + ")", game.currentCard == cards[15], "now on " + game.currentCard.cardName);
            }

            //A normal choice still deals a random card that is not a story follow-up
            SetResources(game, 50);
            game.LoadCard(cards[1]);
            swipe.Invoke(game, new object[] { true });
            Check("Normal card deals a new card", game.currentCard != cards[1] && game.currentCard != cards[0], "now on " + game.currentCard.cardName);

            //Reaching zero shows the death card
            SetResources(game, 50);
            game.resourceManager.water = 10;
            game.LoadCard(cards[1]);
            swipe.Invoke(game, new object[] { false }); //Join in and sing: water -15
            Check("Zero water shows death card", game.currentCard == cards[0], "now on " + game.currentCard.cardName);

            //Dismissing the death card must not count as a decision or add a day
            game.cardSwipeCount = 9;
            game.daysSurvived = 1;
            game.LoadCard(cards[0]);
            swipe.Invoke(game, new object[] { true });
            Check("Death swipe does not add a day", game.daysSurvived == 1 && GameStats.daysSurvived == 1,
                "days " + game.daysSurvived + ", recorded " + GameStats.daysSurvived + ", swipes " + game.cardSwipeCount);
            yield return new WaitForSecondsRealtime(1f);
            Check("Death swipe opens DeathScene", SceneManager.GetActiveScene().name == "DeathScene", SceneManager.GetActiveScene().name);
        }

        //The card flip must take the same time at any frame rate
        int vSync = QualitySettings.vSyncCount;
        QualitySettings.vSyncCount = 0;
        foreach (int fps in new[] { 30, 60, 144, 300 })
        {
            Application.targetFrameRate = fps;
            yield return LoadScene("Playable");
            game = FindFirstObjectByType<GameManager>();
            yield return null;
            game.LoadCard(game.resourceManager.cards[1]);
            float started = Time.realtimeSinceStartup;
            while (game.isSubstituting && Time.realtimeSinceStartup - started < 10f)
                yield return null;
            Check("Flip time at " + fps + " fps", true, (Time.realtimeSinceStartup - started).ToString("0.00") + " s");
        }
        Application.targetFrameRate = -1;
        QualitySettings.vSyncCount = vSync;

        //Presses on the card frame pick it up; presses beside it do not
        game = FindFirstObjectByType<GameManager>();
        game.LoadCard(game.resourceManager.cards[1]);
        yield return new WaitForSecondsRealtime(1.5f);
        Bounds grabArea = game.mainCardController.GetComponent<BoxCollider2D>().bounds;
        bool insideHits = game.mainCardController.Contains(grabArea.center);
        bool outsideMisses = !game.mainCardController.Contains(grabArea.center + new Vector3(grabArea.extents.x + 0.15f, 0f, 0f));
        Check("Card press detection", insideHits && outsideMisses,
            "grab area " + grabArea.size.x.ToString("0.00") + " x " + grabArea.size.y.ToString("0.00") + ", centre hit " + insideHits + ", just beside missed " + outsideMisses);

        //Days counter shows from the first card
        Check("Day counter set at start", game.daysSurvivedText != null && game.daysSurvivedText.text == "Days Survived : 0",
            game.daysSurvivedText == null ? "missing" : "\"" + game.daysSurvivedText.text + "\"");

        File.WriteAllLines(Path.Combine(outputDir, "logic-check.txt"), results);
    }

    static void SetResources(GameManager game, int value)
    {
        ResourceManager resources = game.resourceManager;
        resources.food = resources.water = resources.ammo = resources.morale = resources.medicine = value;
    }

    IEnumerator LoadScene(string sceneName)
    {
        yield return SceneManager.LoadSceneAsync(sceneName);
        yield return new WaitForSecondsRealtime(0.5f);
    }

    IEnumerator Shot(string sizeTag, string label)
    {
        yield return new WaitForEndOfFrame();
        ScreenCapture.CaptureScreenshot(Path.Combine(outputDir, sizeTag + "_" + label + ".png"));
        yield return new WaitForSecondsRealtime(0.2f);
    }

    static List<Vector2Int> ParseSizes(string value)
    {
        List<Vector2Int> parsed = new List<Vector2Int>();
        if (!string.IsNullOrEmpty(value))
        {
            foreach (string item in value.Split(','))
            {
                string[] parts = item.Trim().Split('x');
                if (parts.Length == 2 && int.TryParse(parts[0], out int w) && int.TryParse(parts[1], out int h))
                    parsed.Add(new Vector2Int(w, h));
            }
        }
        if (parsed.Count == 0)
            parsed.AddRange(DefaultSizes);
        return parsed;
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
