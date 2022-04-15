'use strict';

function getAccessToken() {
  const match = /(^|;\s?)\.AspNetCore\.Identity\.Application=(.+?)($|;)/.exec(document.cookie);
  return match && match[2];
}

const uppyInstances = {};

export function create(
  selector,
  dotNetObject,
  { endpoint, debug, maxFileSizeBytes, allowedFileTypes }) {
  return Promise.all([
    System.import('uppy'),
    System.import('jquery')
  ]).then(function ([{ default: Uppy }, { default: $ }]) {
    if (uppyInstances[selector]) {
      throw new Error('There already is an uppy instance for selector ' + selector);
    }

    const uppy = uppyInstances[selector] = new Uppy.Core({
      debug: debug,
      autoProceed: true,
      restrictions: {
        allowedFileTypes: (allowedFileTypes && allowedFileTypes.length > 0) ? allowedFileTypes : null
      }
    });

    uppy.use(Uppy.XHRUpload, {
      endpoint: endpoint,
      timeout: 2 * 60 * 1000, // 2 minutes for the maximum time between progress events,
      // eslint-disable-next-line no-unused-vars
      headers: (file) => ({
        'authorization': 'Bearer ' + getAccessToken()
      }),
      // fix bug in default error handling
      getResponseData: (responseContent) => {
        let response = {}
        try {
          response = JSON.parse(responseContent)
        } catch (err) {
          uppy.log(err, 'error')
        }

        return response
      },
    });

    const loaderSelector = selector + ' .loader.file-upload';
    const fileInputSelector = selector + ' input[type="file"]';

    $(fileInputSelector).on('change', function (event) {
      uppy.cancelAll();

      const files = Array.prototype.slice.call(event.target.files);

      files.forEach(function (file) {
        dotNetObject.invokeMethodAsync('add', file.name);

        if (maxFileSizeBytes && file.size > maxFileSizeBytes) {
          dotNetObject.invokeMethodAsync('error', 'fileSizeExceeded');
          return;
        }

        if (allowedFileTypes && allowedFileTypes.length > 0) {
          const extension = /(?:(\.[^.]+))?$/.exec(file.name)[1];
          if (allowedFileTypes.indexOf(extension) === -1) {
            dotNetObject.invokeMethodAsync('error', 'extensionNotAllowed');
            return;
          }
        }

        try {
          uppy.addFile({
            source: 'file input',
            name: file.name,
            type: file.type,
            data: file
          });

          $(loaderSelector).addClass('show');
        } catch (err) {
          if (err.isRestriction) {
            // handle restrictions
            console.log('Restriction error:', err);
          } else {
            // handle other errors
            console.error(err);
          }
        }
      })
    });

    // eslint-disable-next-line no-unused-vars
    uppy.on('upload-error', function (file, error, response) {
      uppy.reset();
      dotNetObject.invokeMethodAsync('error', 'unknown');
      $(loaderSelector).removeClass('show');
      console.log(error);
    });

    uppy.on('upload-success', function (file, response) {
      uppy.reset();
      dotNetObject.invokeMethodAsync('success', response.body);
      $(loaderSelector).removeClass('show');
    });
  });
}

export function destroy(selector) {
  const uppy = uppyInstances[selector];
  uppy.close();
  uppyInstances[selector] = undefined;
}
