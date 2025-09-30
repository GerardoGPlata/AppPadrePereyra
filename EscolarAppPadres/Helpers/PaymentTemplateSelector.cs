using EscolarAppPadres.Models.Payments;
using Microsoft.Maui.Controls;

namespace EscolarAppPadres.Helpers
{
    public class PaymentTemplateSelector : DataTemplateSelector
    {
        public DataTemplate PendingTemplate { get; set; }
        public DataTemplate PaidTemplate { get; set; }

        protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
        {
            if (item is PaymentItem payment)
            {
                return payment.IsPaid ? PaidTemplate : PendingTemplate;
            }
            return PendingTemplate;
        }
    }
}
