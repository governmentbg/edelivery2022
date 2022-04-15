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
})(jQuery);
