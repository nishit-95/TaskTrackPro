

var email = "v@gmail.com";

$(document).ready(function () {
    LoadProfile(email);

    setTimeout(function () {
        $("#load_screen").hide();
    }, 3000);

    $("select:not([multiple])").kendoDropDownList();

    $("#UserRoleDropdown").kendoDropDownList({
        popup: {
            appendTo: $(".userprofile-menu"),
        },
    });
   

    $("#settings").on("click", function () {
        // Reload profile data when settings is opened
        LoadProfile(email);
    });

    $("#tabstrip").kendoTabStrip({
        animation: {
            open: {
                effects: "fadeIn",
            },
        },
    });

    $("#borederTabstrip").kendoTabStrip({
        animation: {
            open: {
                effects: "fadeIn",
            },
        },
    });

    var dataSource = new kendo.data.DataSource({
        transport: {
            read: {
                url: "https://demos.telerik.com/kendo-ui/service/Products",
                dataType: "jsonp",
            },
        },
    });

    $("#thumbnail_grid").kendoListView({
        dataSource: dataSource,
        selectable: "multiple",
        template: kendo.template($("#thumnail_template").html()),
    });
});


function previewImage() {
    var input = document.getElementById('fileProfileImage');

    if (input.files && input.files[0]) {
        var reader = new FileReader();
        reader.onload = function (e) {
            $('#EditProfileImage').attr('src', e.target.result).show(); // Set src and show image
        };
        reader.readAsDataURL(input.files[0]);
    }
}





// Hide All error message
function hideAllErrorMessages() {
    hideErrorMessage("userNameValidateMessage");
    hideErrorMessage("addressValidateMessage");
    hideErrorMessage("mobileValidateMessage");
    hideErrorMessage("oldpasswordValidateMessage");
    hideErrorMessage("newpasswordValidateMessage");
    hideErrorMessage("copasswordValidateMessage");
}

//displayErrorMessage
function displayErrorMessage(elementId, message) {
    var errorMessageElement = document.getElementById(elementId);
    errorMessageElement.style.color = "red";
    errorMessageElement.innerHTML = message;
}

//hideErrorMessage
function hideErrorMessage(elementId) {
    var errorMessageElement = document.getElementById(elementId);
    errorMessageElement.innerHTML = "";
}





//validateDetails
function validateDetails() {
    var username = validateUserName();
    var address = validateAddress();
    var mobile = validateMobileNumber();

    if (username && address && mobile) {
        UpdateProfile();
        return true;
    } else {
        return false;
    }
}
function validatePassword() {
    var currentPass = validateCurrentPassword();
    var newPass = validateNewPassword();
    var confirmPass = validateConfirmPassword();

    if (currentPass && newPass && confirmPass) {
        resetPassword();
        return true;
    } else {
        return false;
    }
}



// function for update profile
function UpdateProfile() {
    //   @* var email = sessionStorage.getItem("email"); *@
    var fileInput = document.getElementById('fileProfileImage');

    // Access the file object if a file is selected
    var file = null;
    if (fileInput && fileInput.files.length > 0) {
        file = fileInput.files[0];
    }
    var formData = new FormData();
    formData.append("c_UserId", $("#userid").val());
    formData.append("c_UserName", $("#txtuname").val());
    formData.append("c_Mobile", $("#txtmobile").val());  
    formData.append("c_Email", email);
    formData.append("c_Address", $("#txtaddress").val());



    // If a new photo is selected, append it
    if (file) {
        formData.append("photo", file);
        // Send the update request with the form data
        sendUpdateRequest(formData, email);
    } else {
        // If no new photo is selected, fetch the existing photo URL
        var existingPhotoUrl = $('#EditProfileImage').attr('src');

        // Fetch the existing photo data
        fetch(existingPhotoUrl)
            .then(response => response.blob())
            .then(blob => {
                // Append the blob data to the form data
                formData.append("photo", blob, "existing_photo.jpg");
                // Send the update request with the form data
                sendUpdateRequest(formData, email);
            })
            .catch(error => {
                console.error("Error fetching existing photo data:", error);
            });
    }
}


function SetImage() {
    var profilePic = document.getElementById('fileProfileImage');
    var profileUpload = profilePic.value;
    var Extension = profileUpload.substring(profileUpload.lastIndexOf('.') + 1).toLowerCase();

    if (Extension == "jpg" || Extension == "jpeg" || Extension == "png") {

        if (profilePic.files && profilePic.files[0]) {
            var reader = new FileReader();

            reader.onload = function (e) {
                $('#EditProfileImage').attr('src', e.target.result);
            }
            reader.readAsDataURL(profilePic.files[0]);

            tempImage = event.target.files[0];
            var filePath = $('#fileProfileImage').val();
            var tmppath = URL.createObjectURL(event.target.files[0]);
            $(".profileLetterDiv").css("display", "none");
            $(".ImgDiv").css("display", "block");
            $("#EditProfileImage").fadeIn("fast").attr('src', tmppath);
        }
    }
    else {
        alert_popup('Picture must be jpg , jpeg or png !')
        setTimeout(function () {
            document.getElementById("alert-box").style.display = "none";
        }, 2000);
    }
}


function sendUpdateRequest(formData, email) {
    $.ajax({
        url: "http://localhost:5112/api/UserProfileApi/update-profile",
        method: "PUT",
        data: formData,
        processData: false,
        contentType: false,
        success: function (data) {

            UserUpdateNotification();
            LoadProfile(email);
            $('.settings').removeClass('open');
            $('.main-up').removeClass('close');
        },
        error: function (xhr, textStatus, errorThrown) {

            alert(xhr.responseText);
        }
    });
}
// ok

function UserUpdateNotification() {    
    // Show the notification that the user profile has been updated successfully
    var notification = document.getElementById("user-update-notification");
    // Hide the notification after 2 seconds
    notification.innerText = "Profile updated successfully!";
    notification.style.display = "block";
    // Hide message after 3 seconds
    setTimeout(() => {
        notification.style.display = "none";
    }, 6000);
}

function resetEventProfile() {
    // Clear the values of the input fields
    $("#txtuname").val("");
    $("#txtaddress").val("");
    $("#txtmobile").val("");
}


function validateMobileNumber() {
    var mobileNumber = document.getElementById("txtmobile").value.trim();
    var mobileNumberRegex = /^[6-9]\d{9}$/;

    if (mobileNumber === "") {
        displayErrorMessage("mobileValidateMessage", "Please enter a mobile number.");
        return false;
    } else if (!mobileNumberRegex.test(mobileNumber) || mobileNumber.length !== 10) {
        displayErrorMessage("mobileValidateMessage", "Please enter a valid 10-digit mobile number and start with only 6-9.");
        return false;
    } else {
        hideErrorMessage("mobileValidateMessage");
        return true;
    }
}



function validateUserName() {
    var username = document.getElementById("txtuname").value.trim();
    var usernameRegex = /^[a-zA-Z ]+$/; // Allow spaces in the username
    if (username === "") {
        displayErrorMessage("userNameValidateMessage", "Please enter a user name.");
        return false;
    } else if (!usernameRegex.test(username)) {
        displayErrorMessage("userNameValidateMessage", "Please enter a valid user name (only alphabetic characters are allowed).");
        return false;
    } else {
        hideErrorMessage("userNameValidateMessage");
        return true;
    }
}
function validateAddress() {
    var address = document.getElementById("txtaddress").value.trim();
    var addressRegex = /^[a-zA-Z0-9 ,.-/]+$/;

    if (address === "") {
        displayErrorMessage("addressValidateMessage", "Please enter a Address.");
        return false;
    } else if (!addressRegex.test(address)) {
        displayErrorMessage("addressValidateMessage", "Please enter a valid Address (only alphabetic characters are allowed).");
        return false;
    } else {
        hideErrorMessage("addressValidateMessage");
        return true;
    }
}



//validateCurrentPassword
function validateCurrentPassword() {
    var currentPassword = document.getElementById("txtOldPassword").value.trim();
    // var cpass = sessionStorage.getItem("password");
    var cpass = "123";
    if (currentPassword === "") {
        displayErrorMessage("oldpasswordValidateMessage", "Please Enter Current Password.");
        return false;
    } else if (currentPassword !== cpass) {
        displayErrorMessage("oldpasswordValidateMessage", "Please Enter Currect Old Password.");
        return false;
    }
    else {
        hideErrorMessage("oldpasswordValidateMessage");
        return true;
    }
}

//validateNewPassword
function validateNewPassword() {
    var newPassword = document.getElementById("txtNewPassword").value.trim();
    var passwordRegex = /^(?=.*[A-Z])(?=.*[a-z])(?=.*\d)(?=.*[@#$%^&*!]).{8,15}$/;
    var cpass = sessionStorage.getItem("password");

    if (newPassword === "") {
        displayErrorMessage("newpasswordValidateMessage", "Please enter a new password.");
        return false;
    } else if (!passwordRegex.test(newPassword)) {
        displayErrorMessage("newpasswordValidateMessage", "Please enter a valid password (Password should contain special characters, numbers, uppercase, and lowercase letters. Length must be 8 to 15 characters).");
        return false;
    } else if (newPassword == cpass) {
        displayErrorMessage("newpasswordValidateMessage", "New Password Should Differnt from Current Password.");
        return false;
    }
    else {
        hideErrorMessage("newpasswordValidateMessage");
        return true;
    }
}


// validateConfirmPassword
function validateConfirmPassword() {
    var confirmPassword = document.getElementById("txtConfirmPassword").value.trim();
    var newPassword = document.getElementById("txtNewPassword").value.trim();

    if (confirmPassword === "") {
        displayErrorMessage("copasswordValidateMessage", "Please re-type your new password.");
        return false;
    } else if (confirmPassword !== newPassword) {
        displayErrorMessage("copasswordValidateMessage", "Passwords do not match.");
        return false;
    } else {
        hideErrorMessage("copasswordValidateMessage");
        return true;
    }
}




function ResetProfile() {
    // Clear the values of the input fields
    $("#EditProfileImage").attr('src', '');
    $("#fileProfileImage").val("");
    // Set the default image source
    var defaultImagePath = "/Image/image.png"; // Path relative to your application's URL
    $("#EditProfileImage").attr('src', defaultImagePath);
}

function resetEvent() {
    // Clear the values of the input fields
    $("#txtOldPassword").val("");
    $("#txtNewPassword").val("");
    $("#txtConfirmPassword").val("");
}


function LoadProfile(email) {
    console.log("Loading profile..." + email);
    $.ajax({
        url: "http://localhost:5112/api/UserProfileApi/getuserdata?email=" + email,
        method: "GET",
        success: function (data) {
            console.log(data);
            if (data != null) {
                console.log("data is not null");
                $("#userid").val(data.c_userid);

                $("#lblusername").text(data.c_UserName);
                $("#lblusername1").text(data.c_UserName);
                $("#lblemail").text(data.c_Email);
                $("#lblmobile").text(data.c_Mobile);
                $("#lblrole").text(data.c_Role);
                $("#lbladdress").text(data.c_Address);
                $("#lblgender").text(data.c_Gender);

                $("#txtuname").val(data.c_UserName);
                $("#txtemail").val(data.c_Email);
                $("#txtmobile").val(data.c_Mobile);
                $("#txtaddress").val(data.c_Address);

                // Updating profile image if available
                if (data.c_Image) {

                    $(".ImgDiv").css("display", "block");
                    $(".profileLetterDiv").css("display", "none");
                    let imagePath = '/user_images/' + data.c_Image;

                    $("#ViewProfileImage").attr('src', imagePath);
                    $("#EditProfileImage").attr('src', imagePath);
                } else {

                    $(".ImgDiv").css("display", "block");
                    $(".profileLetterDiv").css("display", "none");
                    var defaultImagePath = "/Image/image.png"; // Path relative to your application's URL
                    $("#ViewProfileImage").attr('src', defaultImagePath);
                    $("#EditProfileImage").attr('src', defaultImagePath);
                }
            } else {
                console.log("User profile data not found.");
            }
        },
        error: function (err) {
            console.log("Error fetching user profile data:", err);
        }
    });
}

function PasswordUpdateNotification(){
    var notification = document.getElementById("user-update-notification");
    // Hide the notification after 2 seconds
    notification.innerText = "Password updated successfully!";
    notification.style.display = "block";
    // Hide message after 3 seconds
    setTimeout(() => {
        notification.style.display = "none";
    }, 6000);
}



// function for reset password
function resetPassword() {
    // Gather data from input fields
    var currentPassword = $("#txtOldPassword").val();
    console.log("old password "+currentPassword);
    var newPassword = $("#txtNewPassword").val();
    var confirmPassword = $("#txtConfirmPassword").val();
    // var email = sessionStorage.getItem("email");

    // Prepare data to send to the server
    var userData = {
        c_Email: email, // Assuming you store email in sessionStorage
        c_Password: newPassword,
        c_UserName: "user",
        c_Mobile: "user",
        c_Role: "user",
        c_Image: "user",
        currentPassword: currentPassword
    };
    // Send AJAX request to reset password
    $.ajax({
        url: "http://localhost:5112/api/UserProfileApi/reset-password?currentPassword=" +currentPassword,
        method: "PUT",
        contentType: "application/json",
        data: JSON.stringify(userData),
        success: function (response) {
            // Handle success response
            resetEvent();
            $('.settings').removeClass('open');
            $('.main-up').removeClass('close');
            console.log("Password reset successfully:", response);
            sessionStorage.setItem("password", newPassword);
            console.log(sessionStorage.getItem("password"));
            PasswordUpdateNotification();
            // LoadProfile(email);
        },
        error: function (xhr, textStatus, errorThrown) {
            // Handle error response
            console.error("Error resetting password:", errorThrown);

            // Display error message to the user
            alert("Error resetting password: " + xhr.responseText);
        }
    });
}




// Function to toggle visibility of password in the "Old Password" input field
function togglePasswordVisibility() {
    // Retrieve the password input element and the associated icon
    var passwordInput = document.getElementById("txtOldPassword");
    var passwordIcon = document.querySelector(".toggle-password1 i");
    // Check the current type of the password input field
    if (passwordInput.type === "password") {
        passwordInput.type = "text";
        passwordIcon.classList.remove("k-i-eye");
        passwordIcon.classList.add("k-i-uneye");
    } else {
        passwordInput.type = "password";
        passwordIcon.classList.remove("k-i-uneye");
        passwordIcon.classList.add("k-i-eye");
    }
}

function togglePasswordVisibility1() {
    var passwordInput = document.getElementById("txtNewPassword");
    var passwordIcon = document.querySelector(".toggle-password2 i");

    if (passwordInput.type === "password") {
        passwordInput.type = "text";
        passwordIcon.classList.remove("k-i-eye");
        passwordIcon.classList.add("k-i-uneye");
    } else {
        // If the password is currently visible, hide it

        passwordInput.type = "password";
        // Change the icon to represent the hidden password
        passwordIcon.classList.remove("k-i-uneye");
        passwordIcon.classList.add("k-i-eye");
    }
}


function togglePasswordVisibility2() {
    var passwordInput = document.getElementById("txtConfirmPassword");
    var passwordIcon = document.querySelector(".toggle-password3 i");

    if (passwordInput.type === "password") {
        passwordInput.type = "text";
        passwordIcon.classList.remove("k-i-eye");
        passwordIcon.classList.add("k-i-uneye");
    } else {
        passwordInput.type = "password";
        passwordIcon.classList.remove("k-i-uneye");
        passwordIcon.classList.add("k-i-eye");
    }
}