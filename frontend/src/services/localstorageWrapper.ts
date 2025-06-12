const BASE_KEY = "aioniq_v1_";


const getKey = function (ext: string): string {
    return BASE_KEY + ext.toLowerCase();
}


const setItem = function(key: string, data: any) {
    if (data instanceof Object) {
        data = JSON.stringify(data);
    }
    localStorage.setItem(getKey(key), data);
}

const getItem = function(key: string): null|any {
    return localStorage.getItem(getKey(key));
}

export default {
    getItem: getItem,
    setItem: setItem,
}
