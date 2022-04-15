'use strict';

export function create(selector, options, initialValue) {
  return System.import('datepicker').then(function () {
    return System.import('datepicker/bg');
  }).then(function () {
    $(selector).datepicker(options);
    $(selector).datepicker('update', initialValue);

    // dispatch a real event as blazor listens to those
    var dispatching = false;
    $(selector).on('change', function () {
      if (dispatching) {
        return;
      }
      dispatching = true;
      this.dispatchEvent(new Event('change'));
      dispatching = false;
    })
  });
}

export function clear(selector) {
  return System.import('datepicker').then(function () {
    $(selector).datepicker('clearDates')
  });
}

export function destroy(selector) {
  return System.import('datepicker').then(function () {
    $(selector).datepicker('destroy');
  });
}
