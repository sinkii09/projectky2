
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;


public class UserManager
{

    private string updateUserRankUrl = "http://localhost:3000/users/updateRank";
    private string updateNameOrPasswordUrl = "http://localhost:3000/users/updateUser";
    private string loginUrl = "http://localhost:3000/auth/login";
    private string registerUrl = "http://localhost:3000/auth/register";
    private string authentiCatewithCustomIdUrl = "http://localhost:3000/auth/authenticate-custom-id";
    

    string accessToken;
    public string AccessToken
    {
        get => accessToken;
        set => accessToken = value;
    }
    public UserManager(string accessToken="")
    {
        this.accessToken = accessToken;
    }

    private IEnumerator LoginRequest(string username, string password, Action<LoginResponse> loginSuccessCallback, Action finishLogin)
    {
        string jsonRequestBody = $"{{\"username\":\"{username}\", \"password\":\"{password}\"}}";
        byte[] requestBodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonRequestBody);
        using (UnityWebRequest request = new UnityWebRequest(loginUrl, "POST"))
        {
            request.uploadHandler = new UploadHandlerRaw(requestBodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            yield return request.SendWebRequest();
            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("Login successful!");
                string responseJson = request.downloadHandler.text;
                LoginResponse loginResponse = JsonUtility.FromJson<LoginResponse>(responseJson);
                loginSuccessCallback?.Invoke(loginResponse);
            }
            else
            {
                Debug.LogError($"Login failed: {request.error}");
                finishLogin?.Invoke();
            }
        }
    }
    public void Login(string username, string password, Action<LoginResponse> loginSuccesCallBack, Action finishLogin)
    {
        CoroutineRunner.Instance.StartCoroutine( LoginRequest(username, password,loginSuccesCallBack,finishLogin));
    }
    public void Register(CreateUserDto userDto, Action<LoginResponse> successRegister, Action<string> failedRegister)
    {
        CoroutineRunner.Instance.StartCoroutine(RegisterRequest(userDto, successRegister, failedRegister));
    }
    private IEnumerator RegisterRequest(CreateUserDto userDto,Action<LoginResponse> successRegister, Action<string> failedRegister )
    {
        string json = JsonUtility.ToJson(userDto);
        byte[] bodyRaw = new System.Text.UTF8Encoding().GetBytes(json);
        using (UnityWebRequest request = new UnityWebRequest(registerUrl, "POST"))
        {
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            yield return request.SendWebRequest();
            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("Register successful!");
                string responseJson = request.downloadHandler.text;
                LoginResponse loginResponse = JsonUtility.FromJson<LoginResponse>(responseJson);
                successRegister?.Invoke(loginResponse);
            }
            else
            {
                Debug.LogError($"Register failed: {request.error}");
                failedRegister?.Invoke(request.downloadHandler.text);
            }
        }
    }
    public async Task<AuthenticateResponse> AuthenticateWithUnityRequestAsync(CustomAuthDto dto)
    {
        string json = JsonUtility.ToJson(dto);
        byte[] bodyRaw = new System.Text.UTF8Encoding().GetBytes(json);
        UnityWebRequest request = new UnityWebRequest(authentiCatewithCustomIdUrl, "POST");
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        var operation = request.SendWebRequest();
        while (!operation.isDone)
        {
            await Task.Yield();
        }

        if (request.result == UnityWebRequest.Result.Success)
        {
            string responseJson = request.downloadHandler.text;
            Debug.Log(responseJson);
            AuthenticateResponse response = JsonUtility.FromJson<AuthenticateResponse>(responseJson);
            
            return response;
        }
        else
        {
            throw new Exception("Error authenticate with user data: " + request.error);
        }
    }
    public IEnumerator UpdateUserDataRequest(UpdateUserPartialDto userDto)
    {
        string json = JsonUtility.ToJson(userDto);
        UnityWebRequest request = new UnityWebRequest(updateUserRankUrl, "PATCH");
        byte[] bodyRaw = new System.Text.UTF8Encoding().GetBytes(json);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("Authorization", "Bearer " + accessToken);
        yield return request.SendWebRequest();
        if (request.result == UnityWebRequest.Result.Success)
        {
            Debug.Log("User data updated successfully.");
        }
        else
        {
            Debug.LogError("Error updating user data: " + request.error);
        }
    }
    public void UpdateData(UserData userData)
    {
        UpdateUserPartialDto dto = ConvertDataToDto(userData);
        CoroutineRunner.Instance.StartCoroutine(UpdateUserDataRequest(dto));
    }
    UpdateUserPartialDto ConvertDataToDto(UserData userData)
    {
        UpdateUserPartialDto dto = new UpdateUserPartialDto();
        dto.userId = userData.userId;
        dto.userName = userData.userName;
        dto.rating = userData.Rating;
        dto.rankPoints = userData.RankPoints;
        return dto;
    }
    public void UpdateUser(Action<string,bool> result,string name = "",string oldpassword = "", string newpassword = "")
    {
        CoroutineRunner.Instance.StartCoroutine(UpdateNameOrPasswordRequest(name,oldpassword, newpassword, result));      
    }
    private IEnumerator UpdateNameOrPasswordRequest(string name,string oldPassword, string newPassword,Action<string,bool> result)
    {
        string jsonRequestBody = $"{{\"name\":\"{name}\",\"oldpassword\":\"{oldPassword}\", \"newpassword\":\"{newPassword}\"}}";
        byte[] requestBodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonRequestBody);
        using (UnityWebRequest request = new UnityWebRequest(updateNameOrPasswordUrl, "PATCH"))
        {
            request.uploadHandler = new UploadHandlerRaw(requestBodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Authorization", $"Bearer {accessToken}");
            yield return request.SendWebRequest();
            if (request.result == UnityWebRequest.Result.Success)
            {
                result?.Invoke("Update Success",true);
            }
            else
            {
                result?.Invoke($"Update Failed \n {request.downloadHandler.text}",false);
            }
        }
    }
    public void FindUserByNameOrId(string input, Action<GetPlayerResponse> successGetUser, Action<string> failedGetUser)
    {
        CoroutineRunner.Instance.StartCoroutine(FindUserRequest(input, successGetUser, failedGetUser));
    }
    private IEnumerator FindUserRequest(string userInput,Action<GetPlayerResponse> successGetUser,Action<string> failedGetUser)
    {
        string findPlayerUrl = $"http://localhost:3000/users/{userInput}";
        UnityWebRequest request = UnityWebRequest.Get(findPlayerUrl);
        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("Authorization", $"Bearer {accessToken}");
        yield return request.SendWebRequest();
        if (request.result == UnityWebRequest.Result.Success)
        {

            string responseJson = request.downloadHandler.text;
            GetPlayerResponse response = JsonUtility.FromJson<GetPlayerResponse>(responseJson);
            successGetUser?.Invoke(response);
        }
        else
        {
            failedGetUser?.Invoke(request.downloadHandler.text);
        }
        
    }
    public void FetchFriendList(Action<List<FriendData>> success, Action<string> failed)
    {
        CoroutineRunner.Instance.StartCoroutine(FetchFriendListRequest(success, failed));
    }
    IEnumerator FetchFriendListRequest(Action<List<FriendData>> success, Action<string> failed)
    {
        string url = $"http://localhost:3000/friends/friendList";
        UnityWebRequest request = UnityWebRequest.Get(url);
        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("Authorization", $"Bearer {accessToken}");
        yield return request.SendWebRequest();
        if (request.result == UnityWebRequest.Result.Success)
        {

            string responseJson = request.downloadHandler.text;
            List<FriendData> friendDatas = JsonUtility.FromJson<FriendList>("{\"requests\":" + responseJson + "}").requests;
            success?.Invoke(friendDatas);
        }
        else
        {
            failed?.Invoke(request.downloadHandler.text);
        }
    }

    public void FetchFriendRequestList(Action<List<FriendData>> success, Action<string> failed)
    {
        CoroutineRunner.Instance.StartCoroutine(FetchRequestList(success, failed));
    }
    IEnumerator FetchRequestList(Action<List<FriendData>> success,Action<string> failed)
    {
        string url = $"http://localhost:3000/friends/requests";
        UnityWebRequest request = UnityWebRequest.Get(url);
        request.SetRequestHeader("Authorization", "Bearer " + accessToken);
        yield return request.SendWebRequest();
        if (request.result == UnityWebRequest.Result.Success)
        {
            string jsonResponse = request.downloadHandler.text;
            FriendRequestList friendRequestList = JsonUtility.FromJson<FriendRequestList>("{\"requests\":" + jsonResponse + "}");
            success?.Invoke(friendRequestList.requests);
        }
        else
        {
            failed?.Invoke(request.downloadHandler.text);
        }
    }

    internal void AcceptFriendRequest(string text, Action<string,bool> result)
    {
        FriendAcceptDto dto = new FriendAcceptDto();
        dto.friendId = text;
        CoroutineRunner.Instance.StartCoroutine(SendAcceptFriendRequest(dto, result));
    }
    IEnumerator SendAcceptFriendRequest(FriendAcceptDto dto, Action<string,bool> result)
    {
        string url = $"http://localhost:3000/friends/accept";
        string json = JsonUtility.ToJson(dto);
        byte[] bodyRaw = new System.Text.UTF8Encoding().GetBytes(json);
        using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
        {
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Authorization", "Bearer " + accessToken);
            yield return request.SendWebRequest();
            if (request.result == UnityWebRequest.Result.Success)
            {
                result?.Invoke("Add friend Success", true);
            }
            else
            {
                result?.Invoke("Add friend Failed", false);
            }
        }
    }
    internal void DeniedFriendRequest(string text, Action<string,bool> result)
    {
        FriendAcceptDto dto = new FriendAcceptDto();
        dto.friendId = text;
        CoroutineRunner.Instance.StartCoroutine(SendDeniedFriendRequest(dto, result));
    }
    IEnumerator SendDeniedFriendRequest(FriendAcceptDto dto, Action<string,bool> result)
    {
        string url = $"http://localhost:3000/friends/denied";
        string json = JsonUtility.ToJson(dto);
        byte[] bodyRaw = new System.Text.UTF8Encoding().GetBytes(json);
        using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
        {
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Authorization", "Bearer " + accessToken);
            yield return request.SendWebRequest();
            if (request.result == UnityWebRequest.Result.Success)
            {
                result?.Invoke("denied request Success",true);
            }
            else
            {
                result?.Invoke("denied request Failed",false);
            }
        }
    }

    internal void DeleteFriend(string text, Action<string, bool> result)
    {
        FriendAcceptDto dto = new FriendAcceptDto();
        dto.friendId = text;
        CoroutineRunner.Instance.StartCoroutine(SendDeleteFriendRequest(dto, result));
    }
    IEnumerator SendDeleteFriendRequest(FriendAcceptDto dto, Action<string,bool> result)
    {
        string url = $"http://localhost:3000/friends/delete";
        string json = JsonUtility.ToJson(dto);
        byte[] bodyRaw = new System.Text.UTF8Encoding().GetBytes(json);
        using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
        {
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Authorization", "Bearer " + accessToken);
            yield return request.SendWebRequest();
            if (request.result == UnityWebRequest.Result.Success)
            {
                result?.Invoke("delete friend Success",true);
            }
            else
            {
                result?.Invoke("delete friend Failed",false);
            }
        }
    }

    internal void AddFriend(string nameText, Action<string, bool> result)
    {
        FriendRequestDto dto = new FriendRequestDto();
        dto.friendName = nameText;
        CoroutineRunner.Instance.StartCoroutine(SendAddFriendRequest(dto, result));
    }
    IEnumerator SendAddFriendRequest(FriendRequestDto dto, Action<string, bool> result)
    {
        string url = $"http://localhost:3000/friends/add";
        string json = JsonUtility.ToJson(dto);
        byte[] bodyRaw = new System.Text.UTF8Encoding().GetBytes(json);
        using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
        {
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Authorization", "Bearer " + accessToken);
            yield return request.SendWebRequest();
            if (request.result == UnityWebRequest.Result.Success)
            {
                result?.Invoke("add friend Success", true);
            }
            else
            {
                result?.Invoke("add friend Failed", false);
            }
        }
    }
}
[System.Serializable]
public class FriendList
{
    public List<FriendData> requests;
}
[System.Serializable]
public class FriendRequestList
{
    public List<FriendData> requests;
}
[System.Serializable]
public class FriendData
{
    public string id;
    public string name;
    public string _id;
}
[System.Serializable]
public class FriendAcceptDto
{
    public string friendId;
}[System.Serializable]
public class FriendRequestDto
{
    public string friendName;
}
[System.Serializable]
public class UpdateUserPartialDto
{
    public string userId;
    public string userName;
    public int? rating; 
    public int? rankPoints; 
}

[System.Serializable]
public class Payload
{
    public string sub;
    public string username;
    public string ingameName;
}

[System.Serializable]
public class LoginResponse
{
    public string access_token;
    public Payload payload;
}
[System.Serializable]
public class GetPlayerResponse
{
    public string id;
    public string ingameName;
}
[System.Serializable]
public class AuthenticateResponse
{
    public int expiresIn;
    public string idToken;
    public string sessionToken;
    public User user;
    public string userAuthId;

    public class User
    {
        public bool disabled;
        public ExternalIds externalIds;
        public string id;

        public class ExternalIds
        {
            public string externalId;
            public string providerId;
        }
    }
}
[System.Serializable]
public class CustomAuthDto
{
    public string externalId;
    public bool signInOnly = false;
}
[System.Serializable]
public class CreateUserDto
{
    public string username;
    public string password;
    public string name;
}
