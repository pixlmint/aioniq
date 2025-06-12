import { defineStore } from 'pinia'
import axios from 'axios'

interface State {
    clientId: string,
    initDone: boolean,
    user: UserInformation|null,
}

export type UserInformation = {
    id: string,
    email: string,
    name: string,
    picture: string,
    token: string,
}

const LOCALSTORAGE_USERINFO_KEY = "aioniq_v1_user";

const getUserInformation = function(): UserInformation | null {
    const localstorageItem = localStorage.getItem(LOCALSTORAGE_USERINFO_KEY);
    if (localstorageItem === null) {
        return null;
    } else {
        return JSON.parse(localstorageItem);
    }
}

const setUserInformation = function(userInformation: UserInformation) {
    localStorage.setItem(LOCALSTORAGE_USERINFO_KEY, JSON.stringify(userInformation));
}

const loadUserInformation = async function() {

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
            const self = this;
            window.setTimeout(function() {
                self.initDone = true;
            }, 2000);
        },
    },
})

