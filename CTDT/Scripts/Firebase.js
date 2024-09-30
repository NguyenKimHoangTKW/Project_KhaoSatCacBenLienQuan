 async function getFirebaseConfig() {
            try {
                const response = await fetch('/Firebase/GetConfig');
                const firebaseConfig = await response.json();
                return firebaseConfig;
            } catch (error) {
                console.error("Failed to fetch Firebase configuration:", error);
                throw error;
            }
        }

        async function initializeFirebase() {
            try {
                const firebaseConfig = await getFirebaseConfig();
                firebase.initializeApp(firebaseConfig);
            } catch (error) {
                console.error("Firebase initialization failed:", error);
            }
        }

        async function loginWithGoogle() {
            try {
                await initializeFirebase();

                const loadingAlert = Swal.fire({
                    title: 'Đang xác thực và đăng nhập...',
                    allowOutsideClick: false,
                    didOpen: () => {
                        Swal.showLoading();
                    }
                });

                const provider = new firebase.auth.GoogleAuthProvider();
                // Đặt chính sách lưu trữ phiên làm việc
                await firebase.auth().setPersistence(firebase.auth.Auth.Persistence.LOCAL);
                const result = await firebase.auth().signInWithPopup(provider);
                const user = result.user;
                const idToken = await user.getIdToken();

                const response = await fetch('/Login/LoginWithGoogle', {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/json'
                    },
                    body: JSON.stringify({ token: idToken })
                });

                const data = await response.json();

                Swal.close();

                if (data.success) {
                    Swal.fire({
                        icon: 'success',
                        title: 'Đăng nhập thành công',
                        showConfirmButton: false,
                        timer: 2000
                    }).then(() => {
                        window.location.href = "/";
                    });
                } else {
                    Swal.fire({
                        icon: 'error',
                        title: 'Đăng nhập thất bại!',
                        text: data.message || 'Có lỗi xảy ra, vui lòng thử lại sau.'
                    });
                }
            } catch (error) {
                console.error("Login with Google failed:", error);

                Swal.close();

                Swal.fire({
                    icon: 'error',
                    title: 'Đăng nhập thất bại!',
                    text: 'Có lỗi xảy ra, vui lòng thử lại sau.'
                });
            }
        }