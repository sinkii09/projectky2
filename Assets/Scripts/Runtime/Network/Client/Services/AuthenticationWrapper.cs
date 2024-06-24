using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEditor.PackageManager;
using UnityEngine;

public enum AuthState
{
    NotAuthenticated,
    Authenticating,
    Authenticated,
    Error,
    TimedOut
}
public class AuthenticationWrapper 
{
    public static AuthState AuthorizationState { get; private set; } = AuthState.NotAuthenticated;

    public static async Task<AuthState> DoAuth(string userId,int tries = 5)
    {
        //If we are already authenticated, just return Auth
        if (AuthorizationState == AuthState.Authenticated)
        {
            return AuthorizationState;
        }

        if (AuthorizationState == AuthState.Authenticating)
        {
            Debug.LogWarning("Cant Authenticate if we are authenticating or authenticated");
            await Authenticating();
            return AuthorizationState;
        }
        await SignInAnonymouslyAsync(userId,tries);

        Debug.Log($"Auth attempts Finished : {AuthorizationState.ToString()}");

        return AuthorizationState;
    }

    //Awaitable task that will pass the clientID once authentication is done.
    public static string PlayerID()
    {
        return AuthenticationService.Instance.PlayerId;
    }

    //Awaitable task that will pass once authentication is done.
    public static async Task<AuthState> Authenticating()
    {
        while (AuthorizationState == AuthState.Authenticating || AuthorizationState == AuthState.NotAuthenticated)
        {
            await Task.Delay(200);
        }
        return AuthorizationState;
    }
    static async Task SignInAnonymouslyAsync(string userId,int maxRetries)
    {
        CustomAuthDto dto = new CustomAuthDto();
        dto.externalId = userId;
        dto.signInOnly = false;
        AuthorizationState = AuthState.Authenticating;
        var tries = 0;
        while (AuthorizationState == AuthState.Authenticating && tries < maxRetries)
        {
            try
            {
                if (AuthenticationService.Instance.SessionTokenExists)
                {
                    await AuthenticationService.Instance.SignInAnonymouslyAsync();
                    Debug.Log("Cached user sign-in succeeded!");
                }
                else
                {
                    try
                    {
                        AuthenticateResponse response = await UserManager.Instance.AuthenticateWithUnityRequestAsync(dto);
                        //Debug.Log(response.ToString());
                        AuthenticationService.Instance.ProcessAuthenticationTokens(response.idToken,response.sessionToken);
                        Debug.Log("authenticating");
                    }
                    catch (System.Exception ex)
                    {
                        Debug.LogError("Error authenticate with user data: " + ex);
                    }
                }
                //To ensure staging login vs non staging
                if (AuthenticationService.Instance.IsSignedIn && AuthenticationService.Instance.IsAuthorized)
                {
                    AuthorizationState = AuthState.Authenticated;
                    break;
                }
            }
            catch (AuthenticationException ex)
            {
                // Compare error code to AuthenticationErrorCodes
                // Notify the player with the proper error message
                Debug.LogError(ex);
                AuthorizationState = AuthState.Error;
            }
            catch (RequestFailedException exception)
            {
                // Compare error code to CommonErrorCodes
                // Notify the player with the proper error message
                Debug.LogError(exception);
                AuthorizationState = AuthState.Error;
            }

            tries++;
            await Task.Delay(1000);
        }

        if (AuthorizationState != AuthState.Authenticated)
        {
            Debug.LogWarning($"Player was not signed in successfully after {tries} attempts");
            AuthorizationState = AuthState.TimedOut;
        }
    }

    public static void SignOut()
    {
        AuthenticationService.Instance.SignOut(false);
        AuthorizationState = AuthState.NotAuthenticated;
    }
}

