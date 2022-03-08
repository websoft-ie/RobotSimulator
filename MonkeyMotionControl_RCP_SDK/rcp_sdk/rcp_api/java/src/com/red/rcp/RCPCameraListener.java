/********************************************************************************
 * This file is part of the RCP SDK Release 6.62.0
 * Copyright (C) 2009-2018 RED.COM, LLC.  All rights reserved.
 *
 * For technical support please email rcpsdk@red.com.
 *
 * "Source Code" means the accompanying software in any form preferred for making
 * modifications. "Source Code" does not include the accompanying strlcat.c and
 * strlcpy.c software and examples/qt/common/qt/serial software.
 * 
 * "Binary Code" means machine-readable Source Code in binary form.
 * 
 * "Approved Recipients" means only those recipients of the Source Code who have
 * entered into the RCP SDK License Agreement with RED.COM, LLC. All
 * other recipients are not authorized to possess, modify, use, or distribute the
 * Source Code.
 *
 * RED.COM, LLC hereby grants Approved Recipients the rights to modify this
 * Source Code, create derivative works based on this Source Code, and distribute
 * the modified/derivative works only as Binary Code in its binary form. Approved
 * Recipients may not distribute the Source Code or any modification or derivative
 * work of the Source Code. Redistributions of Binary Code must reproduce this
 * copyright notice, this list of conditions, and the following disclaimer in the
 * documentation or other materials provided with the distribution. RED.COM, LLC
 * may not be used to endorse or promote Binary Code redistributions without
 * specific prior written consent from RED.COM, LLC. 
 *
 * The only exception to the above licensing requirements is any recipient may use,
 * copy, modify, and distribute in any format the strlcat.c and strlcpy.c software
 * in accordance with the provisions required by the license associated therewith;
 * provided, however, that the modifications are solely to the strlcat.c and
 * strlcpy.c software and do not include any other portion of the Source Code.
 * 
 * THE ACCOMPANYING SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
 * EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
 * MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE, TITLE, AND NON-INFRINGEMENT.
 * IN NO EVENT SHALL THE RED.COM, LLC, ANY OTHER COPYRIGHT HOLDERS, OR ANYONE
 * DISTRIBUTING THE SOFTWARE BE LIABLE FOR ANY DAMAGES OR OTHER LIABILITY, WHETHER
 * IN CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE
 * SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 *
 ********************************************************************************/

package com.red.rcp;
/**
 * RCP Camera Listener. Implementation of this interface handles events 
 * dispatched to {@link RCPCamera}.
 * RCP Camera Listener defines the callback interface that will be called 
 * when the connected camera sends data to a {@link RCPCamera} object. This 
 * interface must also be implemented by any classes that instantiate a 
 * RCPCamera object, as the callback methods are delegated to the parent class.
 * 
 */
public interface RCPCameraListener {
	/**
	 * Invoked when a formatted camera integer parameter has been updated.  
	 * For this callback to be invoked, one of the following
	 * callback conditions exist in the SDK callback <code>cur_int_cb</code>:
	 * <ul>
	 * <li><code>display_string_valid</code> is <code>true</code>
	 * <li><code>id == RCP_PARAM_RECORD_STATE</code> and <code>cur_val_valid</code> is <code>true</code>
	 * </ul>
	 * <p>
	 * The param string is delimited in the following manner:
	 * <code>"[Parameter Name];[Value];[Abbreviated Value];[Color]"</code>
	 * 
	 * @param id		identifies the {@link RCPCamera} object
	 * @param param 	defines the parameter values as a string
	 * 
	 */
    public void onUpdate(String id, String param);
	/**
	 * Invoked when a camera integer parameter has been updated. 
	 * For this callback to be invoked, the following conditions
	 * must exist in the SDK callback <code>cur_int_cb</code>: 
	 * <ul>
	 * <li><code>display_string_valid</code> is <code>false</code>
	 * <li><code>id != RCP_PARAM_RECORD_STATE</code> 
	 * <li><code>cur_val_valid</code> is <code>true</code>
	 * </ul>
	 * <p>
	 * @param id		identifies the {@link RCPCamera} object
	 * @param param 	name of the parameter
	 * @param value	 	integer value of the parameter
	 * 
	 */
    public void onUpdateInt(String id, String param, int value);
	/**
	 * Invoked when a camera string parameter has been updated. 
	 * For this callback to be invoked, the following conditions
	 * must exist in the SDK callback <code>cur_list_cb</code>: 
	 * <ul>
	 * <li><code>display_str_in_list</code> is <code>true</code>
	 * </ul>
	 * <p>
	 * The callback will also be invoked if the following conditions
	 * exist in the SDK callback <code>cur_str_cb</code>:
	 <ul>
	 * <li><code>display_str</code> is <code>true</code>
	 * </ul>
	 * <p>
	 * The param string is delimited in the following manner:
	 * <code>"[Parameter Name];[Value];[Abbreviated Value];[Color]"</code>
	 * 
	 * @param id		identifies the {@link RCPCamera} object
	 * @param param 	defines the parameter values as a string
	 * 
	 */
    public void onUpdateString(String id, String param);
	/**
	 * Invoked when edit info of a string parameter has been updated. 
	 * For this callback to be invoked, the following conditions
	 * must exist in the SDK callback <code>cur_str_cb</code>: 
	 * <ul>
	 * <li><code>edit_info_valid</code> is <code>true</code>
	 * </ul>
	 * The editInfo integers are indexed in the following manner:
	 * <code>"[0]=min_length,[1]=max_length,[2]=is_password</code>
	 * The editInfo strings are indexed in the following manner:
	 * <code>"[0]=allowed_chars,[1]=prefix_decoded,[2]=suffix_decoded</code>
	 * 
	 * @param id			identifies the {@link RCPCamera} object
	 * @param paramName		RCP parameter name the edit info values as a string
	 * @param intParams		Integer edit_info parameters passed as an array
	 * @param strParams 	String edit_info parameters passed as an array
	 * 
	 */
	public void onUpdateStringEditInfo(final String id, final String paramName, final int[] intParams, final String[] strParams);

	/**
	 * Invoked when a camera clip list event has occurred. 
	 * When this method is invoked, the clip list status will
	 * either be Loading, Blocked or Done. See the SDK documentation
	 * and source code for enumeration values. 
	 * <p>
	 * If the <code>status</code> is Loading or Blocked, <code>clipList</code> 
	 * will be <code>null</code>. If the <code>status</code> is Done,
	 * <code>clipList</code> could be <code>null</code> if their
	 * are no media clips on the camera.
	 * <p>
	 * If <code>clipList</code> is not <code>null</code>, then the string
	 * will be formatted as follows:
	 * <code>"[Clip 1];[Clip 2];...;[Clip N]"</code> where each clip
	 * is formatted as follows:
	 * <code>"[Index]|[Name]|[Date]|[Time]|[FPS]|[Edge Start Timecode]|[Edge End Timecode]|[TOD Start Timecode]|[TOD End Timecode]"</code>
	 * 
	 * @param id		identifies the {@link RCPCamera} object
	 * @param status 	clip list status: Loading, Blocked, Done
	 * @param clipList	string list of clips formatted as shown above
	 * 
	 */
    public void onUpdateClipList(String id, int status, String clipList);
	/**
	 * Invoked when edit info of a parameter has been updated. 
	 * For this callback to be invoked, the following conditions
	 * must exist in the SDK callback <code>cur_int_cb</code>: 
	 * <ul>
	 * <li><code>edit_info_valid</code> is <code>true</code>
	 * </ul>
	 * <p>
	 * The callback will also be invoked if the following conditions
	 * exist in the SDK callback <code>cur_uint_cb</code>:
	 <ul>
	 * <li><code>edit_info_valid</code> is <code>true</code>
	 * </ul>
	 * <p>
	 * The editInfo string is delimited in the following manner:
	 * <code>"[Parameter Name];[Min Value];[Max Value];[Divider];[Digits];[Step];[Prefix];[Suffix]"</code>
	 * 
	 * @param id		identifies the {@link RCPCamera} object
	 * @param param 	defines the edit info values as a string
	 * 
	 */
    public void onUpdateEditInfo(String id, String editInfo);

    /**
     * Invoked when a camera list has been updated. For this callback to
     * be invoked, the following condition must exist in the SDK callback
     * <code>cur_list_cb</code>:
	 * <ul>
	 * <li><code>list_string_valid</code> is <code>true</code>
	 * </ul>
	 * <p>
	 * The list is then decoded and formatted as follows:
	 * <ul>
	 * <li><code>listHeader  = "[Param Name]|[Length]|[Selected Index]|[Min Valid Flag]|[Min Value]|[Max Valid Flag]|[Max Value]"</code>
	 * <li><code>listStrings = "[Item Str1]|[Item Str2]|...|[Item StrN]"</code>
	 * <li><code>listValues  = "[Item Value1]|[Item Value2]|...|[Item Value3]"</code>
	 * <li><code>listFlags   = "[Send Int Flag]|[Send Uint Flag|[Send Str Flag]|[Update List Only On Close Flag]"</code>
	 * <ul>
	 * 
	 * @param id 			identifies the {@link RCPCamera} object
	 * @param listHeader	identifies the list name and important meta-data
	 * @param listStrings   array of the string names to be displayed in the list
	 * @param listValues  	array of the integer values associated with the string names
	 * @param listFlags		boolean flags pertaining to the list
     */
    public void onUpdateListStrings(String id, String listHeader, String listFlags, String[] listStrings, int[] listValues);

    /**
     * Invoked when a camera histogram has been updated. The histogram contains red,
     * green, blue and luma bins. There are 128 bins per channel with a max bin value of 16. 
     * Also included are clip levels that indicate a percent clip level at the bottom and 
     * top of the pixel range. The text is an abbreviated display string. See the SDK 
     * documentation for detailed information.
     * 
     * @param id 			identifies the {@link RCPCamera} object
     * @param red			integer array of red histogram values
     * @param green 		integer array of green histogram values
     * @param blue			integer array of blue histogram values 
     * @param luma			integer array of luma histogram values
     * @param bottom_clip	percentage of pixels clipped at the bottom of the dynamic range
     * @param top_clip		percentage of pixels clipped at the top of the dynamic range
     * @param text			abbreviated display string 
     */
    public void onUpdateHistogram(String id, int[] red, int[] green, int[] blue, int[] luma, int bottom_clip, int top_clip, String text);
    
    /**
     * Invoked when camera tag information has been updated. The format of the param string
     * is <code>"[Param Name];[Tag Type];[Tag Frame];[Timecode]"</code>
     * 
     * @param id 		identifies the {@link RCPCamera} object
     * @param param		string identifying the tag information
     */
	public void onUpdateTag(String id, String param); 
    
    /**
     * Invoked when a connection state change occurs. The states include Init,
     * Connected, RCP Version Mismatch, RCP Parameter Set Version Mismatch and
     * Communication Error. See the SDK documentation callback <code>cur_state_cb</code>.
     * 
     * @param id		identifies the {@link RCPCamera} object
     * @param param		string containing camera version info
     */
    public void onStateChange(String id, String param);

    /**
     * Invoked when the status of a parameter changes.  See the SDK 
     * documentation callback <code>cur_status_cb</code>.
     * 
     * @param id				identifies the {@link RCPCamera} object
     * @param param				string containing the parameter
     * @param isEnabled	        1 if enabled, 0 if not
     * @param isEnabledValid    1 if the isEnabled field is valid
     * @param isSupported	    1 if supported by hw, 0 if not
     * @param isSupportedValid  1 if the isSupported field is valid
     */
    public void onStatusUpdate(String id, String param, int isEnabled, int isEnabledValid, int isSupported, int isSupportedValid);
    
    /**
     * Invoked when a camera notification is opened, updated, or closed.
     * See the SDK documentation callback <code>notification_cb</code>.
     * 
     * @param id				identifies the {@link RCPCamera} object
     * @param notificationInfo	string detailing info of the notification
     */
    public void onNotificationUpdate(String id, String nID, String title, String message, String notificationInfo);
    
    /**
     * Invoked when audio input and output volumes on the camera are updated. The 
     * audioVu string contains the volumes in db of the four input channels and six
     * output channels in the following format: "[input1];[input2];[input3];[input4];
     * [output0];[output1];[output2];[output3];[output4];[output5];[output6]"
     * See the SDK documentation callback <code>cur_audio_vu_cb</code>.
     * 
     * @param id		identifies the {@link RCPCamera} object
     * @param audioVu	string listing the audio input and output volumes
     */
    public void onAudioVuUpdate(final String id, final String audioVu);
    
    /**
     * Invoked when menu node list is updated on the camera. See the SDK documentation 
     * callback <code>cur_menu_cb</code>.
     * 
     * @param id		    identifies the {@link RCPCamera} object
     * @param childList	    string array listing the child nodes
     * @param ancestorList	string array listing the ancestor nodes
     */
    public void onMenuNodeListUpdate(String id, int nodeId, String[] childList, String[] ancestorList);
    
    /**
     * Invoked when menu node list is updated on the camera. See the SDK documentation 
     * callback <code>cur_menu_cb</code>.
     * 
     * @param id		        identifies the {@link RCPCamera} object
     * @param nodeId 	        identifies the menu node
     * @param isEnabled	        1 if enabled, 0 if not
     * @param isEnabledValid    1 if the isEnabled field is valid
     * @param isSupported	    1 if supported by hw, 0 if not
     * @param isSupportedValid  1 if the isSupported field is valid
     */
    public void onMenuNodeStatusUpdate(String id, int nodeId, int isEnabled, int isEnabledValid, int isSupported, int isSupportedValid);

    /**
     * Invoked when there is a camera connection error detected in the JNI code. The provided
     * message will detail the error that occurred.
     * 
     * @param id 			identifies the {@link RCPCamera} object
     * @param msg			message detailing the error that occurred at the JNI level
     */
    public void onConnectionError(String id, String msg);

    /**
     * Invoked when a connection state change occurs. The camInfo string returns detailed
     * camera information. The format of the camInfo is "[Name];[PIN];[Type];[Version]".
     * 
     * @param id 			identifies the {@link RCPCamera} object
     * @param camInfo		detailed camera information as a string
     */
    public void onCameraInfo(String id, String camInfo);
    
    /**
     * Invoked to update the progress on previously initiated RFTP transfer.
     * File I/O is taken care of in native code.
     * 
     * @param id 				identifies the {@link RCPCamera} object
     * @param uuid				byte array containing the UUID. Same format as rcp_uuid_t
     * @param rftpType			indicates what type of command this was (e.g. store). See rftp_types_t
     * @param rftpError			Error code. See rftp_error_t
     * @param percentComplete	Applicable to STORE and RETRIEVE. From 0 - 100
     * @param dirList			Applicable to LIST command. Array of strings that contains the results of the listing.
     */
    public void onRftpStatusUpdate(String id, byte[] uuid, int rftpType, int rftpError, int percentComplete, String[] dirList);
}
