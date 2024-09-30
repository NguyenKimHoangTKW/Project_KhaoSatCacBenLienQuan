
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
}


function Logout_Session() {
    $.ajax({
        url: '/Account/Logout',
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