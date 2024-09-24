mergeInto(LibraryManager.library, {
    GetSessionIDFromJS: function () {
        var sessionID = getSessionID();
        if (window.unityInstance && window.unityInstance.SendMessage) {
            // 调用 Unity 的 C# 函数
            window.unityInstance.SendMessage('UserSessionManager', 'GetSessionID', sessionID);
        } else {
            console.error("Unity instance is not initialized yet or SendMessage method is not available.");
        }
    },
    CheckRefresh: function () {
        console.log("CheckRefresh method has been called by C#");

        function waitForUnityInstance(callback) {
            if (window.unityInstance && window.unityInstance.SendMessage) {
                callback(true);
            } else {
                console.warn("Unity instance is not initialized yet or SendMessage method is not available.");
                callback(false);
            }
        }

        function checkRefreshAndHandleResult() {
            var isRefresh = checkRefresh();

            if (isRefresh) {
                if (window.unityInstance && window.unityInstance.SendMessage) {
                    // 调用 Unity 的 Logout C# 函数
                    window.unityInstance.SendMessage('StartUIController', 'GoToLogout');
                } else {
                    console.error("Unity instance is not initialized yet or SendMessage method is not available.");
                }
            } else {
                console.warn("isRefresh = false");
            }
        }

        function checkUnityInstance() {
            waitForUnityInstance(function (initialized) {
                if (initialized) {
                    checkRefreshAndHandleResult();
                } else {
                    setTimeout(checkUnityInstance, 100); // 每隔0.1秒检查一次
                }
            });
        }

        checkUnityInstance();
    },
    ForceRefresh: function() {
        forceRefresh();
    },
});
