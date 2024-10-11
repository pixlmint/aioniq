import {createApp} from "vue";
import App from './App.vue'
import VueAxios from 'vue-axios'
import axios from 'axios'
import {createPinia} from "pinia"
import ElementPlus from 'element-plus'
import 'element-plus/dist/index.css'
import vue3GoogleLogin from 'vue3-google-login'
/*import {library} from "@fortawesome/fontawesome-svg-core";
import {
    faArrowRotateRight,
    faBug, faCircle,
    faEraser,
    faHand,
    faHighlighter,
    faPencil,
    faPenRuler,
    faSave,
    faVectorSquare, faTimes, faPlus, faFolderPlus, faFilePdf, faFileCirclePlus, faLock, faUnlock,
    faPen, faPenToSquare, faTrash, faEllipsis, faCaretLeft, faCaretRight, faCaretDown, faHouse, faCirclePlus, faUser,
    faEye, faMoon, faSun, faDownload, faRotate, faGear, faUpload, faCheck, faXmark,
} from "@fortawesome/free-solid-svg-icons";
import {
    faTrello
} from "@fortawesome/free-brands-svg-icons";*/

const pinia = createPinia();

const app = createApp(App)
app.use(VueAxios, axios)
app.use(pinia)
app.use(ElementPlus)

app.use(vue3GoogleLogin, {
    clientId: '804363148845-4mui1b168btsdojprj0ddsf0tfttkmlc.apps.googleusercontent.com',
});

/*library.add(faEraser, faPencil, faHighlighter, faSave, faPenRuler, faBug, faHand, faVectorSquare, faCircle,
    faArrowRotateRight, faTimes, faPlus, faTrello, faFolderPlus, faFilePdf, faFileCirclePlus, faLock, faUnlock,
    faPen, faPenToSquare, faTrash, faEllipsis, faCaretLeft, faCaretRight, faCaretDown, faHouse, faCirclePlus, faUser,
    faEye, faMoon, faSun, faDownload, faRotate, faGear, faUpload, faCheck, faXmark);*/

app.mount('#app')

