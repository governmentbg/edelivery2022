var GlobalFunctions = function () {
    var self = this;

    var modalId = '#modalContainer';
    var modalContent = '#modal-content';
    var modalLoader = '#modalContainer div.loader';

    this.init = function (modalId, modalContent, modalLoader) {
        if (modalId) {
            self.modalId = modalId;
            self.modalContent = modalContent;
            self.modalLoader = modalLoader;
        }

        self.initModalHrefs();
        self.initFile();
    };

    this.initFile = function () {
        $('.file-control').change(function () {
            var prev = $(this).prev('div');
            var file = $(this).val().replace(/\\/g, '/').replace(/.*\//, '');
            $('.file-text', prev).val(file);
        });
    };

    this.initModalHrefs = function () {
        $('[data-modal-href]').myModal();
        $('[data-modal-tagId]').myModal2();
    };

    this.getModalSelector = function () {
        return modalId;
    };

    this.getModalContentSelector = function () {
        return modalContent;
    };

    this.showModal = function () {
        $(modalId).addClass('show');
    };

    this.hideModal = function () {
        $(modalId).removeClass('show');

        $(modalContent).empty();
        $(modalLoader).show();
    };

    this.modalReady = function () {
        $(modalLoader).hide();
    };

    this.setFileUpload = function (fileId, blobId, fileName, hashAlgorithm, hash, size) {
        var fullFileName = fileName + " (" + self.humanFileSize(size) + ")";

        var $fileId = $("#" + fileId);
        var $fileInput = $("#" + fileId + "-file");
        var $fileName = $("#" + fileId + "-text");
        var $fileHash = $("#" + fileId + "-hash");
        var $fileValidation = $("#" + fileId + "-err");
        var $clearButton = $("#" + fileId + "-clear");

        $fileInput.attr('disabled', 'disabled');
        $fileName.val(fullFileName);
        $fileId.val(blobId);
        $fileHash.val(hashAlgorithm + ": " + hash);

        $clearButton.show();

        $fileValidation.removeClass();
        $fileValidation.text('');
        $fileValidation.addClass('field-validation-valid');
    }

    this.messageFields = {
        recipientIds: '#RecipientIds',
        recipientNames: '#RecipientNames'
    };

    this.addMessageRecipient = function (id, name, shouldCloseModal) {
        if (id && name && !checkIsDuplicate(id)) {
            var newProfileId = $(self.messageFields.recipientIds).val()
                ? $(self.messageFields.recipientIds).val() + "|" + id
                : id;
            var newName = $(self.messageFields.recipientNames).val()
                ? $(self.messageFields.recipientNames).val() + "|" + name
                : name;

            $(self.messageFields.recipientIds).val(newProfileId);
            $(self.messageFields.recipientNames).val(newName);

            var div = document.createElement('div');
            div.className = 'recipient-list-item';

            var divText = document.createTextNode(name);

            var divButton = document.createElement('button');
            divButton.className = 'recipient-remove-btn';
            divButton.dataset.recipientId = id;
            var divButtonText = document.createTextNode('\u00A0');
            divButton.appendChild(divButtonText);

            div.appendChild(divText);
            div.appendChild(divButton);

            $('.recipient-list').append(div);
        }

        if (shouldCloseModal) {
            self.hideModal();
        }
    };

    function checkIsDuplicate(val) {
        var recipientIds = $(self.messageFields.recipientIds).val().split('|');

        return recipientIds.indexOf(val) != -1;
    }

    this.removeMessageRecipient = function ($button) {
        var recipientId = $button.data('recipientId')
        var recipientIds = $(self.messageFields.recipientIds).val().split('|');
        var names = $(self.messageFields.recipientNames).val().split('|');
        var index = recipientIds.indexOf(recipientId.toString());

        if (index !== -1) {
            recipientIds.splice(index, 1);
            names.splice(index, 1);
        }

        var newRecipientIds = recipientIds.join('|');
        var newNames = names.join('|');

        $(self.messageFields.recipientIds).val(newRecipientIds);
        $(self.messageFields.recipientNames).val(newNames);

        $button.parent().remove();
    };

    this.removeMessageRecipientModal = function (e) {
        $(e.target).parent().remove();

        if (!$('#modalrecipientslist .recipient-list-item').length) {
            $('#sectionmodalrecipient').hide();

            $('#add-message-recipients')
                .removeClass('btn-primary')
                .addClass('btn-disabled')
                .prop('disabled', true);
        }
    };

    this.removeMessageRecipientSelect2 = function (e, selectorId) {
        var id = $(e.target).attr('data-id');
        var option = $('#modalcontacts #' + selectorId + ' option[value="' + id + '"]');
        var selector = $('#modalcontacts #' + selectorId);

        option.prop('selected', false);
        selector.trigger('change');
        this.removeMessageRecipientModal(event);
    }

    this.addMessageRecipientModal = function (id, name, profileType, selectorId) {
        if (id && name && !$("#sectionmodalrecipient *[data-id='" + id + "']").length) {

            var div = document.createElement('div');
            div.className = 'recipient-list-item';
            div.dataset.id = id;
            div.dataset.name = name;
            div.dataset.type = profileType;

            var divText = document.createTextNode(name);

            var divButton = document.createElement('button');
            divButton.className = 'recipient-remove-btn';
            divButton.setAttribute('data-id', id);
            divButton.setAttribute('onclick', 'gf.removeMessageRecipientSelect2(event, "' + selectorId + '");');
            var divButtonText = document.createTextNode('\u00A0');
            divButton.appendChild(divButtonText);

            div.appendChild(divButton);
            div.appendChild(divText);

            $('#sectionmodalrecipient').show();
            $('#modalrecipientslist').append(div);
            $('#add-message-recipients')
                .removeClass('btn-disabled')
                .addClass('btn-primary')
                .prop('disabled', false);
        }
    };

    this.grantAccessToProfile = function (row, shouldCloseModal) {
        if (row.includes("RevokeAccess")) {
            var content = $(row);
            $('table#profile-access-content tbody tr:first').after(content);
            content.fadeIn(200);

            if (shouldCloseModal) {
                self.hideModal();
            }
        }
    };

    this.showNotify = function (message, type, wait) {
        alertify.notify(message, type, wait);
    };

    this.showSuccess = function (message, wait) {
        alertify.success(message, wait);
    };

    this.showErorr = function (message, wait) {
        alertify.error(message, wait);
    };

    this.refreshMessagesCount = function (messageUrl, ticketUrl, timeout) {
        function getMessagesCount(messageUrl, ticketUrl) {
            function setCount($element, count) {
                if (!$element.length) {
                    return;
                }

                $element.text(count);

                if (count == 0) {
                    $element.hide();
                }
                else {
                    $element.show();
                }
            }

            function setCountWithIncrement($element, count) {
                if (!$element.length) {
                    return;
                }

                var current = parseInt($element.text(), 10);
                var total = count + current;
                $element.text(total);

                if (total == 0) {
                    $element.hide();
                }
                else {
                    $element.show();
                }
            }

            if (document.hidden) {
                return;
            }

            var promise1 = $.post(messageUrl, {});

            var promise2 = $.post(ticketUrl, {});

            Promise.all([promise1, promise2]).then(function (values) {
                var messagesCount = values[0];
                var ticketsCount = values[1];

                if (messagesCount.Success) {
                    $.each(messagesCount.Profiles, function (key, value) {
                        // profile dropdown
                        var profileMessageCounters = $('nav.nav-select-profile span.number[data-profileid=' + value.ProfileId + ']');
                        setCount($(profileMessageCounters[0]), value.NewMessages);
                        setCount($(profileMessageCounters[1]), value.NewSEOSMessages);

                        // left menu
                        setCount(
                            $('ul.left-nav-menu span.number[data-type=delivery][data-profileid=' + value.ProfileId + ']'),
                            value.NewMessages);
                        setCount(
                            $('ul.left-nav-menu span.number[data-type=seos][data-profileid=' + value.ProfileId + ']'),
                            value.NewSEOSMessages);

                        // big blocks on profile homepage
                        if (value.IsCurrentProfile) {
                            setCount($('#message-count'), value.NewMessages);
                            setCount($('#seos-message-count'), value.NewSEOSMessages);
                        }
                    });
                }

                if (ticketsCount.Success) {
                    $.each(ticketsCount.Profiles, function (key, value) {
                        // profile dropdown
                        var profileTicketCounters = $('nav.nav-select-profile span.number[data-profileid=' + value.ProfileId + ']');
                        setCountWithIncrement($(profileTicketCounters[0]), value.TicketsCount);

                        // left menu
                        setCount(
                            $('ul.left-nav-menu span.number[data-type=tickets][data-profileid=' + value.ProfileId + ']'),
                            value.TicketsCount);

                        // big blocks on profile homepage
                        if (value.IsCurrentProfile) {
                            setCount($('#tickets-count'), value.TicketsCount);
                        }
                    });
                }
            })
        }

        getMessagesCount(messageUrl, ticketUrl);

        setInterval(function () {
            getMessagesCount(messageUrl, ticketUrl);
        }, timeout);
    };

    this.humanFileSize = function (bytes) {
        var thresh = 1024;
        var dp = 1;

        if (Math.abs(bytes) < thresh) {
            return bytes + ' B';
        }

        var units = ['KB', 'MB', 'GB', 'TB'];
        var u = -1;
        var r = Math.pow(10, dp);

        do {
            bytes /= thresh;
            ++u;
        } while (Math.round(Math.abs(bytes) * r) / r >= thresh && u < units.length - 1);

        return bytes.toFixed(dp) + ' ' + units[u];
    }

    return {
        init: self.init,
        showModal: self.showModal,
        hideModal: self.hideModal,
        modalReady: self.modalReady,
        getModalSelector: self.getModalSelector,
        getModalContentSelector: self.getModalContentSelector,

        messageFields: self.messageFields,
        addMessageRecipient: self.addMessageRecipient,
        removeMessageRecipient: self.removeMessageRecipient,
        addMessageRecipientModal: self.addMessageRecipientModal,
        removeMessageRecipientModal: self.removeMessageRecipientModal,
        removeMessageRecipientSelect2: self.removeMessageRecipientSelect2,

        grantAccessToProfile: self.grantAccessToProfile,
        showNotify: self.showNotify,
        showSuccess: self.showSuccess,
        showError: self.showErorr,
        refreshMessagesCount: self.refreshMessagesCount,
        humanFileSize: self.humanFileSize,
        setFileUpload: self.setFileUpload
    };
};

var gf = new GlobalFunctions();

$(document).ready(function () {
    gf.init('#modalContainer', '#modal-content', '#modalContainer div.loader');
});
