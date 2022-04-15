$(document).ready(function () {
    $('button.btn-tabs-ham').click(function () {
        $("#nav-login-reg").toggleClass("collapsed");
    });

    $('button.btn-close').click(function () {
        $(this).closest('.wrapper').addClass('hidden');
    });

    //TABS NAVIGATION

    $('.btn_homeTab1').click(function () {
        $(this).siblings('li').removeClass('active');
        $(this).addClass('active');
        $('#nav-login-reg').addClass('collapsed');
        $('#homeTab1-1').siblings('.home-tabs-target').removeClass('show');
        $('#homeTab1-1').addClass('show');
    });

    $('.btn_homeTab2').click(function () {
        $(this).siblings('li').removeClass('active');
        $(this).addClass('active');
        $('#nav-login-reg').addClass('collapsed');
        $('#homeTab2').siblings('.home-tabs-target').removeClass('show');
        $('#homeTab2').addClass('show');
    });

    $('.btn_homeTab3').click(function () {
        $(this).siblings('li').removeClass('active');
        $(this).addClass('active');
        $('#nav-login-reg').addClass('collapsed');
        $('#homeTab3').siblings('.home-tabs-target').removeClass('show');
        $('#homeTab3').addClass('show');
    });

    //select profile

    $('.btn-select-profile').click(function () {
        $(this).toggleClass('selected');
        $('.nav-select-profile').toggleClass('selected');
    });

    $('.btn-left-nav-menu').click(function () {
        $('.left-nav-menu').toggleClass('show');
        $(".left-nav-wrapper").toggleClass("collapsed");
    });

    $('.nav-select-profile li').click(function () {
        $('.btn-select-profile').removeClass('selected');
        $(this).toggleClass('selected');
        $(this).siblings('li').removeClass('selected');
        $('.nav-select-profile').toggleClass('selected');
    });

    $('.tabs-nav button').click(function () {
        $(this).addClass('active');
        $(this).siblings('button').removeClass('active');
    });
});

