
#include <vector>
#include <mutex>
#include "rcp_api/rcp_api.h"

#define DLL __declspec(dllexport)

rcp_camera_connection_t* m_rcpConnection = 0;
rcp_cur_int_cb_data_t* _data = NULL;
rcp_discovery_cam_info_list_t* cam_list = NULL;

std::recursive_mutex** rcp_mutex = new std::recursive_mutex *[RCP_MUTEX_COUNT];

void rcp_mutex_lock(rcp_mutex_t id)
{
	rcp_mutex[id]->lock();
}

void rcp_mutex_unlock(rcp_mutex_t id)
{
	rcp_mutex[id]->unlock();
}

void init_mutex() {
	for (int i = 0; i < RCP_MUTEX_COUNT; i++)
	{
		rcp_mutex[i] = new std::recursive_mutex();
	}
}



extern "C"
{

	typedef rcp_error_t(__stdcall* SendRCPCallback)(char*, int);

	// id, 
	// int cur_val_valid, 
	// int32_t cur_val, 
	// int display_str_valid, 
	// const char* display_str_decoded, 
	// rcp_param_status_t display_str_status, 
	// int display_str_in_list
	typedef void(__stdcall* IntReceivedCallback)(int, int, int, int, char*, int, int);

	// id
	// const char * list_string
	// int32_t min_val
	// int32_t max_val
	// int min_val_valid
	// int max_val_valid
	// int display_str_in_list
	typedef void(__stdcall* ListReceivedCallback)(int id, int list_string_valid, char* list_string, int min_val, int max_val,
		int min_val_valid, int max_val_valid, int display_str_in_list);

	// id
	// char * display_str_abbr_decoded
	// rcp_param_status_t display_str_status

	typedef void(__stdcall* StringReceivedCallback)(int id, char* display_str_abbr_decoded, int display_str_status);
	typedef void(__stdcall* StopStateTimerCallback)();
	typedef void(__stdcall* SetAppOutOfDateCallback)(bool);
	typedef void(__stdcall* StartExternalTimerCallback)();
	typedef void(__stdcall* DropConnectionCallback)();
	typedef void(__stdcall* WriteDatagramCallback)(char* data, int size);


	SendRCPCallback _sendRCPCallback = NULL;
	IntReceivedCallback _intReceivedCallback = NULL;
	ListReceivedCallback _listReceivedCallback = NULL;
	StringReceivedCallback _stringReceivedCallback = NULL;
	StopStateTimerCallback _stopStateTimerCallback = NULL;
	SetAppOutOfDateCallback _setAppOutOfDateCallback = NULL;
	StartExternalTimerCallback _startExternalTimerCallback = NULL;
	DropConnectionCallback _dropConnectionCallback = NULL;
	WriteDatagramCallback _writeDatagramCallback = NULL;

	rcp_error_t sendData(const char* data, size_t len, void* user_data)
	{
		return _sendRCPCallback((char*)data, (int)len);
	}

	void intReceived(const rcp_cur_int_cb_data_t* data, void* user_data)
	{
		_data = (rcp_cur_int_cb_data_t*)data;
		_intReceivedCallback((int)data->id, data->cur_val_valid, data->cur_val,
			data->display_str_valid, (char*)data->display_str_decoded,
			(int)data->display_str_status, data->display_str_in_list);
	}

	void listReceived(const rcp_cur_list_cb_data_t* data, void* user_data)
	{
		_listReceivedCallback((int)data->id, data->list_string_valid, (char*)data->list_string, data->min_val, data->max_val,
			data->min_val_valid, data->max_val_valid, data->display_str_in_list);
	}

	void histogramReceived(const rcp_cur_hist_cb_data_t* data, void* user_data)
	{

	}

	void stringReceived(const rcp_cur_str_cb_data_t* data, void* user_data)
	{
		_stringReceivedCallback((int)data->id, (char*)data->display_str_abbr_decoded, data->display_str_status);
	}

	void stateUpdated(const rcp_state_data_t* data, void* user_data)
	{
		_stopStateTimerCallback();

		switch (data->state)
		{
		case RCP_CONNECTION_STATE_INIT:
			break;

		case RCP_CONNECTION_STATE_CONNECTED:
			printf("RCP connection established", "MainWindow::stateUpdated(const rcp_state_data_t*, void*)");

			_setAppOutOfDateCallback((bool)(data->parameter_set_version_valid && data->parameter_set_newer));

			// Sending a GET RECORD and then verifying if external control is enabled based on whether or not a CURRENT RECORD is sent back
			rcp_get(m_rcpConnection, RCP_PARAM_RECORD_STATE);
			_startExternalTimerCallback();
			break;

		case RCP_CONNECTION_STATE_ERROR_RCP_VERSION_MISMATCH:
			printf("The RCP version is invalid", "MainWindow::stateUpdated(const rcp_state_data_t*, void*)");
			_dropConnectionCallback();
			//QMessageBox::critical(window, "Error", "The RCP version of your camera is invalid. This application only supports RCP version 2.");
			break;

		case RCP_CONNECTION_STATE_ERROR_RCP_PARAMETER_SET_VERSION_MISMATCH:
			printf("The RCP parameter set version is invalid", "MainWindow::stateUpdated(const rcp_state_data_t*, void*)");
			_dropConnectionCallback();
			//QMessageBox::critical(window, "Error", "The RCP parameter set version of your camera is invalid.");
			break;

		case RCP_CONNECTION_STATE_COMMUNICATION_ERROR:
			printf("Communication with camera was lost", "MainWindow::stateUpdated(const rcp_state_data_t*, void*)");
			_dropConnectionCallback();
			//QMessageBox::critical(window, "Error", "Communication with your camera was lost.");
			break;

		default:
			break;
		}
	}

	void broadcastDatagram(const char* data, size_t len, void* user_data)
	{
		_writeDatagramCallback((char*)data, len);
	}

	DLL int Add(int a, int b)
	{
		return a + b;
	}

	DLL void InitMutex() {
		init_mutex();
	}

	DLL void DeleteCameraConnection() {
		if (m_rcpConnection != 0)
		{
			rcp_delete_camera_connection(m_rcpConnection);
			m_rcpConnection = 0;
		}
	}

	DLL void CreateCameraConnection(
		SendRCPCallback sendRCPCallback,
		IntReceivedCallback intReceivedCallback,
		ListReceivedCallback listReceivedCallback,
		StringReceivedCallback stringReceivedCallback,
		StopStateTimerCallback stopStateTimerCallback,
		SetAppOutOfDateCallback setAppOutOfDateCallback,
		StartExternalTimerCallback startExternalTimerCallback,
		DropConnectionCallback dropConnectionCallback
	) {
		_sendRCPCallback = sendRCPCallback;
		_intReceivedCallback = intReceivedCallback;
		_listReceivedCallback = listReceivedCallback;
		_stringReceivedCallback = stringReceivedCallback;
		_stopStateTimerCallback = stopStateTimerCallback;
		_setAppOutOfDateCallback = setAppOutOfDateCallback;
		_startExternalTimerCallback = startExternalTimerCallback;
		_dropConnectionCallback = dropConnectionCallback;

		rcp_camera_connection_info_t info =
		{
			NULL, NULL, NULL,
			sendData, NULL,
			intReceived, NULL,
			0, 0,                               // uint
			listReceived, NULL,
			histogramReceived, NULL,
			stringReceived, NULL,
			0, 0,                               // clip list
			0, 0,                               // frame tagged
			0, 0,                               // status
			0, 0,                               // notification
			0, 0,                               // audio vu
			0, 0,                               // menu tree
			0, 0,                               // menu tree node status
			0, 0,                               // rftp status
			0, 0,                               // user set
			0, 0,                               // user get
			0, 0,                               // user current
			0, 0,                               // user metadata
			0, 0,                               // default int
			0, 0,                               // default uint
			0, 0,                               // action list
			0, 0,                               // key mapping
			stateUpdated, NULL,
		};

		m_rcpConnection = rcp_create_camera_connection(&info);
	}

	DLL void RCPDiscoveryStart(WriteDatagramCallback writeDatagramCallback) {
		_writeDatagramCallback = writeDatagramCallback;
		rcp_discovery_start(broadcastDatagram, NULL);
	}

	DLL void RCPGetList(int id) {
		if (m_rcpConnection != NULL)
			rcp_get_list(m_rcpConnection, (rcp_param_t)id);
	}

	DLL char* RCPGetLabel(int id) {
		char* label = (char*)rcp_get_label(m_rcpConnection, (rcp_param_t)id);
		return label;
	}

	DLL void RCPGet(int id) {
		rcp_get(m_rcpConnection, (rcp_param_t)id);
	}

	DLL void RCPProcessData(char * data, int length) {
		if (m_rcpConnection)
			rcp_process_data(m_rcpConnection, (const char*)data, length);
	}

	DLL void RCPDiscoveryStep() {
		rcp_discovery_step();
	}

	DLL void RCPDiscoveryProcessData(char* data, int len, char* from_ipv4) {
		rcp_discovery_process_data((const char*)data, len, (const char*)from_ipv4);
	}
	struct group
	{
		std::string groupName;
		int userCount;
		std::string userNames;
	};

	struct groupList
	{
		int count;
		group* groups;
	};

	DLL groupList* getGroupList() {

		static group b[] =
		{
			{"dd", 1, "dd"}
		};

		static groupList a[] =
		{
			{ 1, b },
			{ 2, b },
			{ 3, b }
		};

		return a;
	}

	struct CamInfo
	{
		char* ip_address;
		char* id;
		char* camInterface;
		char* pin;
	};

	//DLL CamInfoList* RCPDiscoveryGetList(int * length) {
	//	CamInfoList camInfo;
	//	CamInfoList *camInfoList = new CamInfoList[2];
	//	for (int i = 0; i < 2; i++) {
	//		camInfo.ip_address = (char*)"2";
	//		camInfo.id = (char*)"1";
	//		camInfo.camInterface = (char*)"122";
	//		camInfo.pin = (char*)"1we";
	//		camInfoList[i] = camInfo;
	//	}
	//	*length = 2;
	//	return camInfoList;
	//}


	DLL CamInfo* RCPDiscoveryGetList(int* length) {

		cam_list = rcp_discovery_get_list();
		const rcp_discovery_cam_info_list_t* cur = cam_list;
		std::vector<char*> addresses;
		std::vector<char*> ids;
		std::vector<char*> caminterfaces;
		std::vector<char*> pins;

		while (cur)
		{
			std::string camInterface;
			switch (cur->info.rcp_interface)
			{
			case RCP_INTERFACE_UNKNOWN:
				camInterface = "???";
				break;

			case RCP_INTERFACE_BRAIN_SERIAL:
				camInterface = "Brain Serial";
				break;

			case RCP_INTERFACE_BRAIN_GIGABIT_ETHERNET:
				camInterface = "Brain GigE";
				break;

			case RCP_INTERFACE_REDLINK_BRIDGE:
				camInterface = "REDLINK BRIDGE";
				break;

			default:
				break;
			}

			addresses.push_back((char*)cur->ip_address);
			ids.push_back((char*)cur->info.id);
			caminterfaces.push_back((char*)camInterface.c_str());
			pins.push_back((char*)cur->info.pin);

			cur = cur->next;
		}

		CamInfo camInfo;
		CamInfo *camInfoList = new CamInfo[addresses.size()];
		for (int i = 0; i < addresses.size(); i++) {
			camInfo.ip_address = addresses[i];
			camInfo.id = ids[i];
			camInfo.camInterface = caminterfaces[i];
			camInfo.pin = pins[i];
			camInfoList[i] = camInfo;
		}
		*length = addresses.size();

		return camInfoList;
	}

	DLL void RCPDiscoveryFreeList() {
		rcp_discovery_free_list(cam_list);
	}

	DLL void RCPDiscoveryEnd() {
		rcp_discovery_end();
	}

	typedef void(__stdcall* CallbackTest)(int);
	typedef char* (__stdcall* GetFilePathCallback)(char* filter);

	DLL void DoWork(CallbackTest progressCallback)
	{
		int counter = 0;

		for (; counter <= 100; counter++)
		{
			// do the work...

			if (progressCallback)
			{
				// send progress update
				progressCallback(counter);
			}
		}
	}

	DLL void ProcessFile(GetFilePathCallback getPath)
	{

		if (getPath)
		{
			// get file path...
			char* path = getPath((char*)"Text Files|*.txt");
			// open the file for reading
			FILE* file = fopen(path, "r");
			// read buffer
			char line[1024];

			// print file info to the screen
			printf("File path: %s\n", path ? path : "N/A");
			printf("File content:\n");

			while (fgets(line, 1024, file) != NULL)
			{
				printf("%s", line);
			}

			// close the file
			fclose(file);
		}
	}
}