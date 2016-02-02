#include "pch.h"
#include "PlayReadyHelper.h"

using namespace MediaHelpers;
using namespace Platform;
using namespace Windows::Media::Protection::PlayReady;

PlayReadyHelper::PlayReadyHelper()
{
}

// MICROSOFT PROVIDED: BEGIN

Platform::IBox<Windows::Foundation::DateTime>^ PlayReadyHelper::GetLicenseExpirationDate(IPlayReadyLicense^ license)
{
	//
	// The following code is a workaround for a bug in PlayReady.
	//
	Platform::Object^ boxedExpirationDate = license->ExpirationDate;
	if (boxedExpirationDate != nullptr)
	{
		return ((IBox<Windows::Foundation::DateTime>^)boxedExpirationDate)->Value;
	}
	return nullptr;
}
