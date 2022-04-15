var FileUploader = function () {
    var self = this;

    //ajax file uploader with progress bar
    self.initAjaxUploadWithProgress = function (
        uploadUrl,
        mainId,
        errorMessages,
        fieldName,
        maxUploads,
        maxUploadSize,
        allowedExtensions) {

        var errBox = $(mainId + '-err');
        var progress = $(mainId + '-progress');
        var text = $(mainId + '-text');
        var btn = $(mainId + '-btn');
        var successClass = 'field-validation-valid file-valid';
        var errorClass = 'field-validation-error';

        var options = {
            button: btn[0],
            url: uploadUrl,
            name: fieldName,
            multiple: true,
            multipart: true,
            maxUploads: maxUploads,
            maxSize: maxUploadSize,
            hoverClass: 'btn-hover',
            focusClass: 'active',
            disabledClass: 'disabled',
            responseType: 'json',
            startXHR: function () {
                progress.addClass('show');
                btn.prop('disabled', true);
            },
            onSubmit: function (filename, ext) {
                text.val('');
                errBox.text('');
                errBox.removeClass(successClass);
                errBox.addClass(errorClass);
                errBox.hide();
            },
            onError: function () {
                progress.removeClass('show');
                errBox.text(errorMessages.fileNotUploaded);
                errBox.show();
            },
            onSizeError: function () {
                errBox.text(errorMessages.maxFileSize);
                errBox.show();
            },
            onExtError: function () {
                errBox.text('Invalid file extension.');
                errBox.show();
            },
            onComplete: function (file, response, btn) {
                progress.removeClass('show');
                $(btn).prop('disabled', false);

                if (!response) {
                    errBox.text(errorMessages.fileNotUploaded);
                    errBox.show();
                    return;
                }

                //disabled malware
                if (response.IsSuccessfulScan === false
                    && (response.ErrorReason === 'MalwareScanDisabled' || response.ErrorReason === 4)) {

                    text.val(response.FileName);
                    $(mainId).val(response.DbItemId);
                    return;
                }

                if (response.IsMalicious === true) {
                    errBox.text(errorMessages.maliciousFile);
                    errBox.show();
                    return;
                }

                if (response.IsSuccessfulScan === false) {
                    errBox.text(response.Message);
                }
                else if (response.IsSuccessfulScan && response.IsMalicious === false) {
                    errBox.removeClass(errorClass);
                    errBox.addClass(successClass);
                    errBox.text(errorMessages.notMaliciousFile);
                }

                errBox.show();
                text.val(response.FileName);
                $(mainId).val(response.DbItemId);
            }
        };

        if (allowedExtensions && allowedExtensions.length > 0) {
            options["allowedExtensions"] = allowedExtensions;
        }

        var uploader = new ss.SimpleUpload(options);
    };

    return {
        initAjaxUploadWithProgress: self.initAjaxUploadWithProgress
    };
};

var fileUploader = new FileUploader();
