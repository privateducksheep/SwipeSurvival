mergeInto(LibraryManager.library, {
  // How many canvas pixels make up one CSS pixel, so the UI can stay readable on phones.
  SwipeSurvival_CanvasPixelsPerCssPixel: function () {
    var canvas = Module.canvas;
    if (canvas && canvas.clientWidth > 0) {
      return canvas.width / canvas.clientWidth;
    }
    return window.devicePixelRatio || 1;
  }
});
