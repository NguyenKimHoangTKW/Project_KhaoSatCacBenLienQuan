function signIn() {
    let oauth2Endpoint = "https://accounts.google.com/o/oauth2/v2/auth";

    let form = document.createElement('form');
    form.setAttribute('method', 'GET');
    form.setAttribute('action', oauth2Endpoint);

    let params = {
        "client_id": "183100229430-394rpj38v42o4kfgum7hvnjplnv3ebrl.apps.googleusercontent.com",
        "redirect_uri": "https://localhost:44301/trang-chu",
        "response_type": "token",
        "scope": "https://www.googleapis.com/auth/userinfo.profile https://www.googleapis.com/auth/userinfo.email",
        "include_granted_scopes": 'true',
        "state": 'pass-through-value'
    };

    for (var p in params) {
        let input = document.createElement('input');
        input.setAttribute('type', 'hidden');
        input.setAttribute('name', p);
        input.setAttribute('value', params[p]);
        form.appendChild(input);
    }
    document.body.appendChild(form);
    form.submit();

}
let params = {};
let regex = /([^&=]+)=([^&]*)/g, m;
while (m = regex.exec(location.href)) {
    params[decodeURIComponent(m[1])] = decodeURIComponent(m[2]);
}

if (Object.keys(params).length > 0) {
    localStorage.setItem('authInfo', JSON.stringify(params));
}

window.history.pushState({}, document.title);

let info = JSON.parse(localStorage.getItem('authInfo'));

if (info && info['access_token']) {
    fetch("https://www.googleapis.com/oauth2/v3/userinfo", {
        headers: {
            "Authorization": `Bearer ${info['access_token']}`
        }
    })
        .then((response) => response.json())
        .then((userInfo) => {
            Session_Login(userInfo.email, userInfo.given_name, userInfo.family_name, userInfo.picture)
        })
        .catch((error) => {
            console.error("Error fetching user info:", error);
        });
}
function Session_Login(email, firstname, lastname, urlimage) {
    $.ajax({
        url: '/api/session_login',
        type: 'POST',
        dataType: 'JSON',
        contentType: "application/x-www-form-urlencoded; charset=UTF-8",
        data: {
            email: email,
            firstName: firstname,
            lastName: lastname,
            avatarUrl: urlimage
        },
        success: function (res) {
            if (typeof load_he_dao_tao === "function") {
                load_he_dao_tao();
            }
            $("#nav-placeholder").load('/InterfaceClient/load_chuc_nang_nguoi_dung');
        }
    })
}


function Logout_Session() {
    $.ajax({
        url: '/api/clear_session',
        type: 'POST',
        success: function (res) {
            if (res.success) {
                localStorage.removeItem('authInfo');
                location.href = "/";
            }
        }
    })
}

function logout() {
    fetch("https://oauth2.googleapis.com/revoke?token=" + info['access_token'], {
        method: 'POST',
        headers: {
            "Content-type": 'application/x-www-form-urlencoded'
        }
    })
        .then(() => {
            Logout_Session();
        })
        .catch((error) => {
            console.error("Error during logout:", error);
        });
}