'use strict';

function timeout(func, delay) {
  let cancel;
  var promise = new Promise(resolve => {
    let cancelled = false;
    cancel = () => {
      cancelled = true;
    };

    return setTimeout(() => {
      if (!cancelled) {
        resolve(func());
      }
    }, delay);
  });

  promise.cancel = cancel;

  return promise;
}

const MaxBackoff = 15 * 60 * 1000; // 15 minutes

function poll(callback, delay, immediate) {
  let promise = null;
  let cancelled = false;
  let backoff = delay;
  const hasVisibilityApi = typeof document.hidden !== 'undefined';

  const func = () => {
    Promise.resolve(callback()).then(
      () => {
        // reset backoff
        backoff = delay;
        promise = timeout(func, delay);
      },
      () => {
        backoff = Math.min(backoff * 2, MaxBackoff);
        promise = timeout(func, backoff);
      }
    );
  };

  if (!hasVisibilityApi || !document.hidden) {
    promise = timeout(func, immediate ? 0 : delay);
  }

  const onVisibilityChange = () => {
    if (document.hidden) {
      if (promise) {
        promise.cancel();
        promise = null;
      }
    } else {
      if (!promise) {
        func();
      }
    }
  };

  if (hasVisibilityApi) {
    document.addEventListener('visibilitychange', onVisibilityChange);
  }

  return () => {
    if (!cancelled) {
      if (promise) {
        promise.cancel();
        promise = null;
      }

      if (hasVisibilityApi) {
        document.removeEventListener('visibilitychange', onVisibilityChange);
      }

      cancelled = true;
    }
  };
}

System.import("jquery").then(function ({ default: $ }) {
  poll(
    () =>
      $.ajax(window.pathBase).then(
        undefined,
        (jqXHR) => {
          if (jqXHR.status === 401) {
            location.reload();
            return;
          }

          return Promise.reject(jqXHR);
        }
      ),
    5 * 60 * 1000 // 5 minutes
  );
});
