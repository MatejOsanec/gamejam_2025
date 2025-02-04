#if UNITY_PS4 || UNITY_PS5
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Assertions;

#if UNITY_PS4
using Sony.PS4.SaveData;
#elif UNITY_PS5
using Unity.SaveData.PS5;
using Unity.SaveData.PS5.Core;
#endif

public class SaveDataAsyncEventScheduler {

    private readonly Dictionary<int, Action<ResponseBase>> _requestCallbackMap = new Dictionary<int, Action<ResponseBase>>();
    private readonly Dictionary<FunctionTypes, Action<ResponseBase>> _notificationCallbackMap = new Dictionary<FunctionTypes, Action<ResponseBase>>();

    public SaveDataAsyncEventScheduler() {

        Main.OnAsyncEvent += OnSaveDataAsyncEvent;
    }

    public Task<Response> InvokeAsyncEvent<Request, Response>(Func<Request, Response, int> asyncEvent, Request request, Response response) where Request : RequestBase where Response : ResponseBase {

        TaskCompletionSource<Response> taskCompletionSource = new TaskCompletionSource<Response>();

        Assert.IsTrue(request.Async);

        int requestId = asyncEvent(request, response);

        Assert.IsFalse(_requestCallbackMap.ContainsKey(requestId), $"request with ID {requestId} already assigned");

        void OnAsyncEventFinished(ResponseBase newResponse) {
            taskCompletionSource.TrySetResult(newResponse as Response);
        }

        _requestCallbackMap[requestId] = OnAsyncEventFinished;

        return taskCompletionSource.Task;
    }

    public void RegisterNotification(FunctionTypes functionType, Action<ResponseBase> notificationCallback) {

        if (!_notificationCallbackMap.ContainsKey(functionType)) {
            _notificationCallbackMap[functionType] = notificationCallback;
        }
        else {
            _notificationCallbackMap[functionType] += notificationCallback;
        }
    }

    public void UnregisterNotification(FunctionTypes functionType, Action<ResponseBase> notificationCallback) {

        if (_notificationCallbackMap.ContainsKey(functionType)) {
            _notificationCallbackMap[functionType] -= notificationCallback;
        }
    }

    public void ClearNotification(FunctionTypes functionType) {

        _notificationCallbackMap.Remove(functionType);
    }

    private void OnSaveDataAsyncEvent(SaveDataCallbackEvent callbackEvent) {

        if (callbackEvent.Response.IsErrorCode) {
            Debug.LogError(
                $"received error event from response: {callbackEvent.Response.GetType().ToString()} with return code: {callbackEvent.Response.ReturnCode} and return code value: {callbackEvent.Response.ReturnCodeValue} for API: {callbackEvent.ApiCalled.ToString()}"
            );
        }

        bool hasEventRegisteredCallback = false;
        if (_requestCallbackMap.TryGetValue(callbackEvent.RequestId, out Action<ResponseBase> requestCallback)) {
            hasEventRegisteredCallback = true;
            requestCallback?.Invoke(callbackEvent.Response);
            _requestCallbackMap.Remove(callbackEvent.RequestId);
        }

        if (_notificationCallbackMap.TryGetValue(callbackEvent.ApiCalled, out Action<ResponseBase> notificationCallback)) {
            hasEventRegisteredCallback = true;
            notificationCallback?.Invoke(callbackEvent.Response);
        }

        if (!hasEventRegisteredCallback) {
            Debug.LogWarning($"received event without registered callback from response: {callbackEvent.Response.GetType().ToString()}");
        }
    }
}
#endif // UNITY_PS4 || UNITY_PS5