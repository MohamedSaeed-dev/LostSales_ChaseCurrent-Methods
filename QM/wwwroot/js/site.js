

// Prevent alpha 
var numInputs = document.querySelectorAll(".num");
var inputValue = "";
numInputs.forEach(x => {
    x.addEventListener("input", (e) => {
        inputValue = e.target.value.slice(0, e.target.value.length - 1);
        if (isNaN(e.target.value)) {
            if (inputValue == "") {
                x.value = "";
                inputValue = "";
            }
            else {
                x.value = inputValue;
            }
        }
    })
})


var dropbtn = document.querySelector(".drop");

var demandsArray = Array.from(document.querySelectorAll(".demands"));
var rtcArray = Array.from(document.querySelectorAll(".rtc"));
var otcArray = Array.from(document.querySelectorAll(".otc"));
var scArray = Array.from(document.querySelectorAll(".sc"));

var array = [demandsArray, rtcArray, otcArray, scArray];

var activeElement = "";
var activeElementValue = "";

for (let i = 0; i < array.length; i++) {
    for (let j = 0; j < array[i].length; j++) {
        array[i][j].addEventListener("input", (e) => {
            activeElement = e.target;
            activeElementValue = e.target.value;
        })
    }
}

for (let i = 0; i < array.length; i++) {
    for (let j = 0; j < array[i].length; j++) {
        array[i][j].addEventListener("focus", (e) => {
            activeElement = e.target;
            activeElementValue = e.target.value;
        })
    }
}

dropbtn.addEventListener("click", () => {
    let ind = -1;
    let arr = [];
    for (let i = 0; i < array.length; i++) {
        if (array[i].indexOf(activeElement) !== -1) {
            ind = array[i].indexOf(activeElement);
            arr = array[i];
            break;
        }

    }
    for (let i = ind + 1; i < arr.length; i++) {
        arr[i].value = activeElementValue;
    }

})





var solveBtn = document.querySelector("#solve");
var formControls = document.querySelectorAll(".control");
var formControlsArray = Array.from(formControls);

formControlsArray.forEach(x => {
    x.addEventListener("focus", () => {
        let notNull = formControlsArray.every(z => z.value != "")
        solveBtn.disabled = notNull ? false : true;
    })
})

window.addEventListener("load", () => {
        let notNull = formControlsArray.every(z => z.value != "")
        solveBtn.disabled = notNull ? false : true;
})