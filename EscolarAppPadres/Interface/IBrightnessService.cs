using System;
using System.Threading.Tasks;

namespace EscolarAppPadres.Interface
{
    public interface IBrightnessService
    {
        Task<bool> ChangeBrightness(double brightness, string message);
        double GetCurrentBrightness();
    }
}
