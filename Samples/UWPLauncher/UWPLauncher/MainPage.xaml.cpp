//
// MainPage.xaml.cpp
// Implementation of the MainPage class.
//

#include "pch.h"
#include "MainPage.xaml.h"
#include <ppltasks.h>
#include <fcntl.h>  
#include <io.h>
#include <string>

using namespace UWPLauncher;

using namespace concurrency;
using namespace Platform;
using namespace Windows::ApplicationModel::Core;
using namespace Windows::Foundation;
using namespace Windows::Foundation::Collections;
using namespace Windows::UI::Core;
using namespace Windows::UI::Xaml;
using namespace Windows::UI::Xaml::Controls;
using namespace Windows::UI::Xaml::Controls::Primitives;
using namespace Windows::UI::Xaml::Data;
using namespace Windows::UI::Xaml::Input;
using namespace Windows::UI::Xaml::Media;
using namespace Windows::UI::Xaml::Navigation;
using namespace Windows::UI::ViewManagement;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409
MainPage^ MainPage::Current = nullptr;
MainPage::MainPage()
{
	InitializeComponent();
	MainPage::Current = this;
	Size size(100, 100);
	ApplicationView^ view = ApplicationView::GetForCurrentView();
	auto r = view->TryResizeView(Size(100, 100));
	auto dispatcher = Window::Current->CoreWindow->Dispatcher;
	LaunchWin32App(dispatcher);
}
void MainPage::LaunchWin32App(Windows::UI::Core::CoreDispatcher^ dispatcher)
{
	auto t = create_task(Windows::ApplicationModel::Package::Current->GetAppListEntriesAsync());
	t.then([dispatcher](IVectorView <Windows::ApplicationModel::Core::AppListEntry^>^ entries)
	{
		AppListEntry^ appEntry = nullptr;

		for (AppListEntry^ entry : entries)
		{
			auto info = entry->DisplayInfo;
			if (info->DisplayName == L"WIN32APP")
			{
				appEntry = entry;
				break;
			}
		}

		if (appEntry)
		{
			auto t2 = create_task(appEntry->LaunchAsync());
			t2.then([dispatcher](bool result)
			{
				dispatcher->RunAsync(Windows::UI::Core::CoreDispatcherPriority::Normal, ref new DispatchedHandler([=]()
				{
					Windows::UI::Xaml::Application::Current->Exit();
				}));
			});
		}
	});
}