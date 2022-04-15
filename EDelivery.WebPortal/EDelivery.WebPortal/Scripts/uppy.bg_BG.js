// https://github.com/transloadit/uppy/blob/main/packages/%40uppy/locales/src/bg_BG.js
const bg_BG = {}

bg_BG.strings = {
  exceedsSize: 'Размерът на файла надвишава максимално разрешения размер от',
  failedToUpload: 'Грешка при качване на %{file}',
  uploadFailed: 'Качването неуспешно',
  uploadPaused: 'Качването е паузирано',
  youCanOnlyUploadFileTypes: 'Можете да качване само файлове: %{types}'
}

bg_BG.pluralize = function pluralize (count) {
  if (count === 1) {
    return 0
  }
  return 1
}

if (typeof window !== 'undefined' && typeof window.Uppy !== 'undefined') {
  window.Uppy.locales.bg_BG = bg_BG
}
