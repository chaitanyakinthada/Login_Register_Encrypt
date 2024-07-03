function ValidRegister() {
    let x = document.getElementById("Name").value;
    let y = document.getElementById("Mobile").value;
    let z = document.getElementById("Password").value;
    let x1 = document.getElementById("ConfirmPassword").value;


    if (x == "" || y == "" || z == "" || x1 == "") {
        alert("All fields are required");
        return false;
    }
    else {
        return true;
    }

}

function ValidEmail() {
    let z = document.getElementById("Email").value;

    var re = /\S+@\S+\.\S+/;
    var r = re.test(z);
    if (r == true) {
        return true;
    }
    else {
        alert("Invalid Email");
        return false;
    }

}