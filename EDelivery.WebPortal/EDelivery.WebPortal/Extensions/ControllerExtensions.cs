using System.Web.Mvc;

namespace EDelivery.WebPortal.Extensions
{
    public static class ControllerExtensions
    {
        private const string ModelStateKey = "ModelState";

        public static void SetTempModel<T>(
            this Controller controller,
            T model,
            bool saveModelState)
        {
            controller.TempData[typeof(T).Name] = model;

            if (saveModelState)
            {
                controller.TempData[ModelStateKey] = controller.ModelState;
            }
        }

        public static T GetTempModel<T>(
            this Controller controller,
            bool restoreModelState)
        {
            T vm = default;

            if (controller.TempData[typeof(T).Name] is T model)
            {
                vm = model;

                if (restoreModelState
                    && controller.TempData[ModelStateKey] is ModelStateDictionary modelState)
                {
                    controller.ModelState.Merge(modelState);
                }
            }

            return vm;
        }
    }
}