import { defineStore } from 'pinia'
import { useRouter } from 'vue-router'
import axios from 'axios'
import { ref } from 'vue'

export const useUserStore = defineStore('user', () => {
    const router = useRouter()

    // Data
    const userData = ref<object>({})

    const fetchUserDataFrom = async (code: any) => {
        try {
            localStorage.setItem('gCode', JSON.stringify(code))

            const { data } = await axios.post(
                'https://oauth2.googleapis.com/token',
                {
                    code,
                    client_id: clientId.value,
                    client_secret: clientSecret.value,
                    redirect_uri: location.origin,
                    grant_type: 'authorization_code',
                },
            )

            if (data) {
                const accessToken = data.access_token

                // Fetch user details using the access token
                const userObj = await axios.get(
                    'https://www.googleapis.com/oauth2/v3/userinfo',
                    {
                        headers: {
                            Authorization: `Bearer ${accessToken}`,
                        },
                    },
                )

                if (userObj && userObj.data) {
                    // save copy in storage
                    localStorage.setItem('user', JSON.stringify(userObj.data))
                    userData.value = userObj.data
                }
                else {
                    // Handle the case where userResponse or userResponse.data is undefined
                    console.error('Failed to fetch user data')
                }
            }
        }
        catch (e) {
            console.error('Failed to exchange token', e)
        }
    }

    return {
        // Data
        clientId,
        clientSecret,
        userData,

        // Functions
        fetchUserDataFrom,
    }
})

export default useUserStore

