'use strict';

export function create(selector, options, initialValue, events, eventsDotNetObject) {
  return System.import('select2').then(function () {
    return System.import('select2/bg');
  }).then(function () {
    const onEvents = {};
    if (Array.isArray(events)) {
      for (let event of events) {
        onEvents[event] = function () {
          eventsDotNetObject.invokeMethodAsync(event);
        }
      }
    }

    let initSelection;
    if (options.ajax &&
      options.ajax.url &&
      initialValue
    ) {
      const initialValueArray =
        Array.isArray(initialValue)
          ? initialValue
          : [initialValue];

      if (initialValueArray.length) {
        initSelection = function (element, callback) {
          // add loading indicators
          callback(initialValueArray.map(function (i) {
            return {
              id: i,
              text: '...'
            };
          }));

          $.ajax(options.ajax.url, {
            data: { ids: initialValueArray },
            traditional: true // serialize query as ids=1&ids=2
          }).then(function (data) {
            callback(data);
          });
        };
      }
    }
    
    $(selector).select2(Object.assign(options, { initSelection: initSelection }));
    $(selector).val(initialValue).trigger('change');
    $(selector).on(onEvents);
  });
}

export function getData(selector) {
  return System.import('select2').then(function () {
    const data = $(selector).select2('data');
    return data.map(function (item) {
      return {
        id: item.id,
        text: item.text
      };
    });
  });
}

export function destroy(selector) {
  return System.import('select2').then(function () {
    $(selector).select2('destroy');
  });
}
