
const onSignInClick = () => {
    open('./account/signin', "_self");
}

const onSignOutClick = () => {
    open('./account/signout', "_self");
}

const getInfo = async () => {
    const infoEl = document.getElementById('info');
    infoEl.textContent = "Loading...";

    try {
        const response = await fetch("./account/info");
        const data = await response.json();
        infoEl.textContent = JSON.stringify(data);
    } catch (error) {
        infoEl.textContent = "Error";
    }
}