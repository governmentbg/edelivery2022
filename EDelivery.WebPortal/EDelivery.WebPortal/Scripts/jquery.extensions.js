(function ($) {
    $.ajaxSetup({
        cache: false
    });

    $.fn.mutualExclusive = function (other) {
        var $current = $(this);
        var $other = $(other);

        var isCurrentCheckbox = $current.is('input[type="checkbox"]');
        var isOtherCheckbox = $other.is('input[type="checkbox"]');

        if (isCurrentCheckbox && isOtherCheckbox) {
            $current.on('change', function () {
                var isCurrentChecked = $current.is(':checked');

                if (isCurrentChecked) {
                    var isOtherChecked = $other.is(':checked');

                    if (isOtherChecked) {
                        $other.prop("checked", false);
                    }
                }
            });

            $other.on('change', function () {
                var isOtherChecked = $other.is(':checked');

                if (isOtherChecked) {
                    var isCurrentChecked = $current.is(':checked');

                    if (isCurrentChecked) {
                        $current.prop("checked", false);
                    }
                }
            });
        }
    }

    function resetTabs(btnSelector, tabSelector, activeClass) {
        $(btnSelector).removeClass(activeClass);
        $(tabSelector).removeClass(activeClass);
    }

    function openTab(btn, target, activeClass, cb) {
        $(btn).addClass(activeClass);
        $('#' + target).addClass(activeClass);

        if (cb) {
            cb($('#' + target));
        }
    }

    $.fn.myTabs = function (btnSelector, tabSelector, targetAttribute, activeClass, cb) {
        return this.each(function () {
            $(this).on('click', function () {
                var target = $(this).attr(targetAttribute);

                resetTabs(btnSelector, tabSelector, activeClass);
                openTab(this, target, activeClass, cb);
            });
        });
    };

    $.fn.myUrlTabs = function (btnSelector, tabSelector, targetAttribute, activeClass, cb) {
        var hash = window.location.hash;

        return this.each(function () {

            $(this).on('click', function () {
                var target = $(this).attr(targetAttribute);

                var fragment = $(this).data('fragment');
                var url = new URL(window.location.origin + window.location.pathname + '#' + fragment);
                window.history.pushState(null, null, url);

                resetTabs(btnSelector, tabSelector, activeClass);
                openTab(this, target, activeClass, cb);
            });

            var fragment = $(this).data('fragment');
            if (hash === '#' + fragment) {
                $(this).trigger('click');
            }
        });
    };

    $.fn.myModal = function () {
        return this.each(function () {
            var data = $(this).data('my-modal');

            if (!data) {
                $(this).on('click', function (e) {
                    e.preventDefault();
                    e.stopPropagation();

                    var url = $(this).attr('data-modal-href');
                    var target = gf.getModalContentSelector();

                    //todo clear target html and show loader?

                    gf.showModal();

                    if (url && target) {
                        $(target).load(url, function (data, status, xhr) {
                            if (status == "success") {
                                $.validator.unobtrusive.parse(target + ' form');
                                gf.modalReady();
                            }
                            else if (status == "error" && xhr.status == 400) {
                                gf.hideModal();
                            }
                        });
                    }
                });

                $(this).data('my-modal', 1)
            }
        });
    };

    $.fn.myModal2 = function () {
        return this.each(function () {
            var data = $(this).data('my-modal2');

            if (!data) {
                $(this).on('click', function (e) {
                    e.preventDefault();
                    e.stopPropagation();

                    var contentHolder = $(this).attr('data-modal-tagId');
                    var target = gf.getModalContentSelector();

                    gf.showModal();

                    if (contentHolder && target) {
                        var content = $('#' + contentHolder).html();
                        $(target).html(content);
                        gf.modalReady();
                    }
                });

                $(this).data('my-modal2', 1)
            }
        });
    };

    $.fn.serializeObject = function () {
        var o = {};
        var a = this.serializeArray();
        $.each(a, function () {
            if (o[this.name]) {
                if (!o[this.name].push) {
                    o[this.name] = [o[this.name]];
                }
                o[this.name].push(this.value || '');
            } else {
                o[this.name] = this.value || '';
            }
        });
        return o;
    };

    $.GetUrlParams = function () {
        var url = window.location.search;
        if (url !== '') {
            var qs = url.substring(url.indexOf('?') + 1).split('&');
            for (var i = 0, result = {}; i < qs.length; i++) {
                qs[i] = qs[i].split('=');
                result[qs[i][0]] = decodeURIComponent(qs[i][1]);
            }
            return result;
        }
        return '';
    };

    $.SetUrlParameters = function (data, url) {
        var newUrl = url;
        for (var key in data) {
            if (data.hasOwnProperty(key)) {
                if (data[key] != '') {
                    var loweredKey = key.charAt(0).toLowerCase() + key.substr(1);
                    newUrl.searchParams.set(loweredKey, data[key]);
                }
            }
        }
        return newUrl;
    };
})(jQuery);
