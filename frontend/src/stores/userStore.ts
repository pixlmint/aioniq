import { defineStore } from 'pinia'
import axios from 'axios'
import localStorageWrapper from '@/services/localstorageWrapper'

interface State {
    clientId: string,
    initDone: boolean,
    user: UserInformation | null,
}

export type UserInformation = {
    id: string,
    name: string,
    picture: string,
    token: string,
}

type JwtData = {
    aud: string,
    email: string,
    exp: number,
    iss: string,
    jti: string,
    name: string,
    picture: string,
    prim: string,
    sub: string,
}

const getUserInformation = function(): UserInformation | null {
    const localstorageItem = localStorageWrapper.getItem("user");
    if (localstorageItem === null) {
        return null;
    } else {
        return JSON.parse(localstorageItem);
    }
}

const setUserInformation = function(userInformation: UserInformation) {
    localStorageWrapper.setItem("user", userInformation);
    localStorageWrapper.setItem("jwt", userInformation.token);
}

const loadUserInformation = async function() {
    const storedInfo = getUserInformation();

    const token = storedInfo === null ? null : storedInfo.token;

    return await axios.post("/api/Auth/VerifyLoggedIn", { token: token }).then(response => {
        const tokenData = parseJwt(response.data.token);

        const updatedInfo: UserInformation = {
            id: tokenData.prim,
            name: tokenData.name,
            picture: tokenData.picture,
            token: response.data.token,
        }

        setUserInformation(updatedInfo);

        return updatedInfo;
    }).catch(() => {
        return null;
    });
}


const parseJwt = function(token: string): JwtData {
    const base64Url = token.split('.')[1];
    const base64 = base64Url.replace(/-/g, '+').replace(/_/g, '/');
    const jsonPayload = decodeURIComponent(
        atob(base64).split('').map(c =>
            '%' + ('00' + c.charCodeAt(0).toString(16)).slice(-2)
        ).join('')
    );
    return JSON.parse(jsonPayload);
}

export const useUserStore = defineStore('user', {
    state: (): State => ({
        clientId: "804363148845-4mui1b168btsdojprj0ddsf0tfttkmlc.apps.googleusercontent.com",
        initDone: false,
        user: null,
    }),
    actions: {
        loginOrRegister: async function(code: string, redirectUri: string) {
            axios.post("/api/Auth/HandleOAuthLogin", {
                code: code,
                redirectUri: redirectUri,
            });
        },
        init: async function() {
            this.user = await loadUserInformation();
            this.initDone = true;
            // const self = this;
            // window.setTimeout(function() {
            //     self.initDone = true;
            // }, 2000);
        },
    },
})

