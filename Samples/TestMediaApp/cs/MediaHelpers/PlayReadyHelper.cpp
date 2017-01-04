#include "pch.h"
#include "PlayReadyHelper.h"
#include <wrl.h>
#include <wrl/client.h>
#include <wrl\implements.h>
#include <dxgi.h>
#include <dxgi1_2.h>
#include <dxgi1_3.h>
#include <d3d11_1.h>
#include <d2d1_2.h>
#include "D3d11.h"

using namespace MediaHelpers;
using namespace Microsoft::WRL;
using namespace Windows::Media::Devices;
using namespace Platform;
using namespace Windows::Media::Protection::PlayReady;

PlayReadyHelper::PlayReadyHelper()
{
}

/// <summary>
/// Retrieve the PlayReady license expiration date 
/// The use of this method is a turn around to a PlayReady issue with .Net Native.
/// </summary>
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
/// <summary>
/// Return true if the platform does support Hardware DRM 
/// </summary>
bool PlayReadyHelper::IsHardwareDRMSupported(void)
{
	HRESULT hr;
	ComPtr<IDXGIFactory2> dxgiFactory;
	ComPtr<IDXGIAdapter> dxgiAdapter;
	ComPtr<IDXGIDevice1> dxgiDevice;		
	ComPtr<ID3D11Device>        d3dDevice;
	ComPtr<ID3D11DeviceContext> d3dContext;
	ComPtr<ID3D11VideoDevice> d3dVideoDevice;

	UINT factoryFlags = 0;
	hr = CreateDXGIFactory2(factoryFlags, __uuidof(IDXGIFactory2), &dxgiFactory);
	if (hr == S_OK)
	{
		UINT creationFlags = D3D11_CREATE_DEVICE_BGRA_SUPPORT | D3D11_CREATE_DEVICE_VIDEO_SUPPORT;

		d3dDevice = nullptr;
		UINT i_adapter = 0;
		while (d3dDevice == nullptr)
		{
			hr = dxgiFactory->EnumAdapters(i_adapter++, &dxgiAdapter);
			if (hr == S_OK)
			{
				hr = D3D11CreateDevice(
					dxgiAdapter.Get(),
					D3D_DRIVER_TYPE_UNKNOWN,
					nullptr,
					creationFlags,
					NULL,
					0,
					D3D11_SDK_VERSION,
					&d3dDevice,
					nullptr,
					&d3dContext
					);
				if (FAILED(hr))
					d3dDevice = nullptr;
			}
			else
				break;
		}
		if (hr != S_OK)
			return false;

		hr = d3dDevice.As(&d3dVideoDevice);
		if (hr != S_OK)
			return false;

		UINT count = d3dVideoDevice->GetVideoDecoderProfileCount();
		for (UINT i = 0; i < count; i++)
		{
			GUID DecoderProfile;
			D3D11_VIDEO_CONTENT_PROTECTION_CAPS protectionCaps;
			hr = d3dVideoDevice->GetVideoDecoderProfile(
				i,
				&DecoderProfile
				);
			if (hr == S_OK)
			{
				hr = d3dVideoDevice->GetContentProtectionCaps(
					NULL,
					&DecoderProfile,
					&protectionCaps
					);
				if (hr == S_OK)
				{
					//	D3D11_CONTENT_PROTECTION_CAPS_SOFTWARE = 0x1,
					//	D3D11_CONTENT_PROTECTION_CAPS_HARDWARE = 0x2,
					if (protectionCaps.Caps & D3D11_CONTENT_PROTECTION_CAPS_HARDWARE)
						return true;
				}
			}
		}
	}
	return false;
}