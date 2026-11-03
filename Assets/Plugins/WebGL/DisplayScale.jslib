mergeInto(LibraryManager.library, {
  // Width of the game canvas on the page in CSS pixels, so the game can work out
  // the screen's pixel density and keep the UI readable on phones. (The canvas's
  // own width attribute can still be the browser default while the game starts.)
  SwipeSurvival_CanvasCssWidth: function () {
    var canvas = Module["canvas"];
    if (canvas && canvas.clientWidth > 0) {
      return canvas.clientWidth;
    }
    return window.innerWidth || 0;
  }
});
