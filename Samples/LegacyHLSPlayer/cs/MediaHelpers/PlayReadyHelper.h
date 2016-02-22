#pragma once
using namespace Windows::Media::Protection::PlayReady;
namespace MediaHelpers
{
    public ref class PlayReadyHelper sealed
    {
    public:
		PlayReadyHelper();
		static Platform::IBox<Windows::Foundation::DateTime>^ GetLicenseExpirationDate(IPlayReadyLicense^ license);
		static bool IsHardwareDRMSupported(void);

    };
}
