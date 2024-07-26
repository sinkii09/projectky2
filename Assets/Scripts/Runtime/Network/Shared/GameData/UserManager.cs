
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using UnityEngine;
using UnityEngine.Networking;


public class UserManager : MonoBehaviour
{
    public static UserManager Instance { get; set; }

    string domain = "https://projectky2-bdb1fda54766.herokuapp.com";
    //private string updateUserRankUrl = "https://projectky2-bdb1fda54766.herokuapp.com/users/update-rank-server";
    private string updateNameOrPasswordUrl = "https://projectky2-bdb1fda54766.herokuapp.com/users/updateUser";
    private string loginUrl = "https://projectky2-bdb1fda54766.herokuapp.com/auth/login";
    private string registerUrl = "https://projectky2-bdb1fda54766.herokuapp.com/auth/register";
    private string authentiCatewithCustomIdUrl = "https://projectky2-bdb1fda54766.herokuapp.com/auth/authenticate-custom-id";


    string accessToken;
    public string AccessToken
    {
        get => accessToken;
        set => accessToken = value;
    }
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {

            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }
    private IEnumerator LoginRequest(string username, string password, Action<LoginResponse> loginSuccessCallback, Action<string> finishLogin)
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
                string response = request.downloadHandler.text;
                ErrorRespone data = JsonUtility.FromJson<ErrorRespone>(response);
                finishLogin?.Invoke(data.message);
            }
        }
    }
    public void Login(string username, string password, Action<LoginResponse> loginSuccesCallBack, Action<string> finishLogin)
    {
        StartCoroutine(LoginRequest(username, password, loginSuccesCallBack, finishLogin));
    }
    public void Register(CreateUserDto userDto, Action<LoginResponse> successRegister, Action<string> failedRegister)
    {
        StartCoroutine(RegisterRequest(userDto, successRegister, failedRegister));
    }
    private IEnumerator RegisterRequest(CreateUserDto userDto, Action<LoginResponse> successRegister, Action<string> failedRegister)
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
                string response = request.downloadHandler.text;
                Debug.Log(response);
                ErrorRespone data = JsonUtility.FromJson<ErrorRespone>(response);
                failedRegister?.Invoke(data.message);
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
        UnityWebRequest request = new UnityWebRequest($"{domain}/users/update-rank-server", "PATCH");
        byte[] bodyRaw = new System.Text.UTF8Encoding().GetBytes(json);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
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


    public void ServerUpdateData(UserData userData)
    {
        UpdateUserPartialDto dto = ConvertDataToDto(userData);
        StartCoroutine(UpdateUserDataRequest(dto));
    }
    UpdateUserPartialDto ConvertDataToDto(UserData userData)
    {
        UpdateUserPartialDto dto = new UpdateUserPartialDto();
        dto.userId = userData.userId;
        dto.rating = userData.Rating;
        dto.rankPoints = userData.RankPoints;
        return dto;
    }
    public void ServerGetUserData(string id, Action<UpdateUserPartialDto, bool> callback)
    {
        StartCoroutine(ServerGetUserDataRequest(id, callback));
    }
    private IEnumerator ServerGetUserDataRequest(string id, Action<UpdateUserPartialDto, bool> result)
    {
        using (UnityWebRequest request = new UnityWebRequest($"{domain}/users/get-user-server/:{id}", "GET"))
        {
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Authorization", $"Bearer {accessToken}");
            yield return request.SendWebRequest();
            if (request.result == UnityWebRequest.Result.Success)
            {
                string responseJson = request.downloadHandler.text;
                UpdateUserPartialDto response = JsonUtility.FromJson<UpdateUserPartialDto>(responseJson);
                result?.Invoke(response, true);
            }
            else
            {
                result?.Invoke(null, false);
            }
        }
    }
    public void ClientUpdateUser(Action<string, bool> result, string name = "", string oldpassword = "", string newpassword = "")
    {
        StartCoroutine(UpdateNameOrPasswordRequest(name, oldpassword, newpassword, result));
    }
    private IEnumerator UpdateNameOrPasswordRequest(string name, string oldPassword, string newPassword, Action<string, bool> result)
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
                result?.Invoke("Update Success", true);
            }
            else
            {
                result?.Invoke($"Update Failed \n {request.downloadHandler.text}", false);
            }
        }
    }
    public void FindUserByNameOrId(string input, Action<GetPlayerResponse> successGetUser, Action<string> failedGetUser)
    {
        StartCoroutine(FindUserRequest(input, successGetUser, failedGetUser));
    }
    private IEnumerator FindUserRequest(string userInput, Action<GetPlayerResponse> successGetUser, Action<string> failedGetUser)
    {
        string findPlayerUrl = $"{domain}/users/{userInput}";
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
    public void FetchUserRank(Action<UserRank> success, Action failed)
    {
        StartCoroutine(FetchUserRankRequest(success, failed));
    }
    IEnumerator FetchUserRankRequest(Action<UserRank> success, Action failed)
    {
        string url = $"{domain}/users/get-rank";
        UnityWebRequest request = UnityWebRequest.Get(url);
        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("Authorization", $"Bearer {accessToken}");
        yield return request.SendWebRequest();
        if (request.result == UnityWebRequest.Result.Success)
        {

            string responseJson = request.downloadHandler.text;
            UserRank rank = JsonUtility.FromJson<UserRank>(responseJson);
            success?.Invoke(rank);
        }
        else
        {
            failed?.Invoke();
        }
    }
    public void FetchLeaderboard(Action<List<LeadUser>> success, Action<string> failed)
    {
        StartCoroutine(FetchLeaderboardRequest(success, failed));
    }
    IEnumerator FetchLeaderboardRequest(Action<List<LeadUser>> success, Action<string> failed)
    {
        string url = $"{domain}/users/leader";
        UnityWebRequest request = UnityWebRequest.Get(url);
        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("Authorization", $"Bearer {accessToken}");
        yield return request.SendWebRequest();
        if (request.result == UnityWebRequest.Result.Success)
        {

            string responseJson = request.downloadHandler.text;
            string wrappedJson = "{\"users\":" + responseJson + "}";
            Leaderboard data = JsonUtility.FromJson<Leaderboard>(wrappedJson);
            success?.Invoke(data.users);
        }
        else
        {
            failed?.Invoke(request.downloadHandler.text);
        }
    }
    public void FetchFriendList(Action<List<FriendData>> success, Action<string> failed)
    {
        StartCoroutine(FetchFriendListRequest(success, failed));
    }
    IEnumerator FetchFriendListRequest(Action<List<FriendData>> success, Action<string> failed)
    {
        string url = $"{domain}/friends/friendList";
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
        StartCoroutine(FetchRequestList(success, failed));
    }
    IEnumerator FetchRequestList(Action<List<FriendData>> success, Action<string> failed)
    {
        string url = $"{domain}/friends/requests";
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

    internal void AcceptFriendRequest(string text, Action<string, bool> result)
    {
        FriendAcceptDto dto = new FriendAcceptDto();
        dto.friendId = text;
        StartCoroutine(SendAcceptFriendRequest(dto, result));
    }
    IEnumerator SendAcceptFriendRequest(FriendAcceptDto dto, Action<string, bool> result)
    {
        string url = $"{domain}/friends/accept";
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
    internal void DeniedFriendRequest(string text, Action<string, bool> result)
    {
        FriendAcceptDto dto = new FriendAcceptDto();
        dto.friendId = text;
        StartCoroutine(SendDeniedFriendRequest(dto, result));
    }
    IEnumerator SendDeniedFriendRequest(FriendAcceptDto dto, Action<string, bool> result)
    {
        string url = $"{domain}/friends/denied";
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
                result?.Invoke("denied request Success", true);
            }
            else
            {
                result?.Invoke("denied request Failed", false);
            }
        }
    }

    internal void DeleteFriend(string text, Action<string, bool> result)
    {
        FriendAcceptDto dto = new FriendAcceptDto();
        dto.friendId = text;
        StartCoroutine(SendDeleteFriendRequest(dto, result));
    }
    IEnumerator SendDeleteFriendRequest(FriendAcceptDto dto, Action<string, bool> result)
    {
        string url = $"{domain}/friends/delete";
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
                result?.Invoke("delete friend Success", true);
            }
            else
            {
                result?.Invoke("delete friend Failed", false);
            }
        }
    }

    internal void AddFriend(string nameText, Action<string, bool> result)
    {
        FriendRequestDto dto = new FriendRequestDto();
        dto.friendName = nameText;
        StartCoroutine(SendAddFriendRequest(dto, result));
    }
    IEnumerator SendAddFriendRequest(FriendRequestDto dto, Action<string, bool> result)
    {
        string url = $"{domain}/friends/add";
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

    internal void ClientFetchGameResult(string id, Action<GameSessionResult, List<PlayerResult>> result, Action failed)
    {
        StartCoroutine(FetchResultRequest(id, result, failed));
    }
    IEnumerator FetchResultRequest(string id, Action<GameSessionResult, List<PlayerResult>> result, Action failed)
    {
        string url = $"{domain}/game-sessions/last-session/{id}";
        using (UnityWebRequest request = new UnityWebRequest(url, "GET"))
        {
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Authorization", "Bearer " + accessToken);
            yield return request.SendWebRequest();
            if (request.result == UnityWebRequest.Result.Success)
            {
                string response = request.downloadHandler.text;
                GameSessionResult reponseSession = JsonUtility.FromJson<GameSessionResult>(response);
                var responseList = reponseSession.gameResult;
                result?.Invoke(reponseSession, responseList);
            }
            else
            {
                failed?.Invoke();
            }
        }
    }
    public void FetchShopItems(Action<ShopItemsList> success, Action<string> failed)
    {
        StartCoroutine(FetchShopItemsRequest(success,failed));
    }
    IEnumerator FetchShopItemsRequest(Action<ShopItemsList> success, Action<string> failed)
    {
        string url = $"{domain}/shop/items";
        using (UnityWebRequest request = new UnityWebRequest(url, "GET"))
        {
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Authorization", "Bearer " + accessToken);
            yield return request.SendWebRequest();
            if (request.result == UnityWebRequest.Result.Success)
            {
                string response = request.downloadHandler.text;
                ShopItemsList shopdata = JsonUtility.FromJson<ShopItemsList>("{\"items\":" + response + "}");
                success?.Invoke(shopdata);
            }
            else
            {
                string response = request.downloadHandler.text;
                ErrorRespone data = JsonUtility.FromJson<ErrorRespone>(response);
                failed?.Invoke(data.message);
            }
        }
    }
    public void FetchUserInventory(Action<List<InventoryItem>> success, Action<string> failed)
    {
        StartCoroutine(GetUserInventory(success, failed));
    }
    IEnumerator GetUserInventory(Action<List<InventoryItem>> success, Action<string> failed)
    {
        string url = $"{domain}/users/get-inventory";
        using (UnityWebRequest request = new UnityWebRequest(url, "GET"))
        {
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Authorization", "Bearer " + accessToken);
            yield return request.SendWebRequest();
            if (request.result == UnityWebRequest.Result.Success)
            {
                string response = request.downloadHandler.text;
                InventoryResponse inventoryResponse = JsonUtility.FromJson<InventoryResponse>(response);
                success?.Invoke(inventoryResponse.inventory);
            }
            else
            {
                string response = request.downloadHandler.text;
                ErrorRespone data = JsonUtility.FromJson<ErrorRespone>(response);
                failed?.Invoke(data.message);
            }
        }
    }

    internal void EquipItemRequest(string itemId, Action<InventoryItem> fetchInventorySuccess, Action<string> fetchInventoryFailed)
    {
        StartCoroutine(SendEquipItemRequest(itemId, fetchInventorySuccess, fetchInventoryFailed));
    }
    IEnumerator SendEquipItemRequest(string itemId, Action<InventoryItem> success, Action<string> failed)
    {
        string url = $"{domain}/users/{itemId}/equip-item";
        using (UnityWebRequest request = new UnityWebRequest(url, "PATCH"))
        {
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Authorization", "Bearer " + accessToken);
            yield return request.SendWebRequest();
            if (request.result == UnityWebRequest.Result.Success)
            {
                string response = request.downloadHandler.text;
                InventoryItem item = JsonUtility.FromJson<InventoryItem>(response);
                success?.Invoke(item);
            }
            else
            {
                string response = request.downloadHandler.text;
                ErrorRespone data = JsonUtility.FromJson<ErrorRespone>(response);
                failed?.Invoke(data.message);
            }
        }
    }

    public void GetPlayersEquippedItems(string[] userIds,Action<UserEquippedItemsList> success, Action<string> failed)
    {
        StartCoroutine(GetPlayersEquippedItemsRequest(userIds,success,failed));
    }
    IEnumerator GetPlayersEquippedItemsRequest(string[] userIds, Action<UserEquippedItemsList> success, Action<string> failed)
    {
        string url = $"{domain}/users/get-allplayers-equipped";
        UserIdsRequest requestBody = new UserIdsRequest
        {
            userIds = userIds
        };
        using (UnityWebRequest request = new UnityWebRequest(url, "GET"))
        {
            var jsonBody = JsonUtility.ToJson(requestBody);
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonBody);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Authorization", "Bearer " + accessToken);
            yield return request.SendWebRequest();
            if (request.result == UnityWebRequest.Result.Success)
            {
                string response = request.downloadHandler.text;
                UserEquippedItemsList data = JsonUtility.FromJson<UserEquippedItemsList>("{\"users\":" + response + "}");
                success?.Invoke(data);
            }
            else
            {
                string response = request.downloadHandler.text;
                ErrorRespone data = JsonUtility.FromJson<ErrorRespone>(response);
                failed?.Invoke(data.message);
            }
        }
    }

    internal void PurchaseItem(string itemID, Action purchaseItemSuccess, Action<string> purchaseItemFailed)
    {
        StartCoroutine(PurchaseItemRequest(itemID, purchaseItemSuccess, purchaseItemFailed));
    }
    IEnumerator PurchaseItemRequest(string itemID, Action purchaseItemSuccess, Action<string> purchaseItemFailed)
    {
        string url = $"{domain}/shop/{itemID}/purchase";
        using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
        {
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Authorization", "Bearer " + accessToken);
            yield return request.SendWebRequest();
            if (request.result == UnityWebRequest.Result.Success)
            {
                string response = request.downloadHandler.text;
                InventoryItem item = JsonUtility.FromJson<InventoryItem>(response);
                purchaseItemSuccess?.Invoke();
            }
            else
            {
                string response = request.downloadHandler.text;
                ErrorRespone data = JsonUtility.FromJson<ErrorRespone>(response);
                purchaseItemFailed?.Invoke(data.message);
            }
        }
    }
}
[System.Serializable]
public class UserIdsRequest
{
    public string[] userIds;
}
[Serializable]
public class InventoryResponse
{
    public List<InventoryItem> inventory;
}
[System.Serializable]
public class Leaderboard
{
    public List<LeadUser> users;
}
[System.Serializable]
public class LeadUser
{
    public string _id;
    public string name;
    public string rankpoints;
    public string rank;
}[System.Serializable]
public class UserRank
{
    public string _id;
    public int rankpoints;
    public int rank;
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
    public int rating; 
    public int rankPoints; 
}

[System.Serializable]
public class Payload
{
    public string id;
    public string username;
    public string ingameName;
    public int gold;
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
[Serializable]
public class AccessToken
{
    public string access_token;
}

[Serializable]
public class GameSessionResult
{
    public string sessionId;
    public string gameMode;
    public List<PlayerResult> gameResult;
}
[Serializable]
public class PlayerResult
{
    public string name;
    public string kills;
    public string deaths;
    public int place;
    public int rpEarn;
    public string rank;
}
[Serializable]
public class ErrorRespone
{
    public string message;
    public string error;
}

