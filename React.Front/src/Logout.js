import React/*, { Component,useEffect }*/ from 'react';

import { Navigate } from 'react-router-dom';

export default function Logout(props) {
    //call logout from app.js

    let content = props.isLoggedIn ?
        props.logout()
        : <Navigate to={"/"} />


    return (

        <div>
           
            {/*navigate to the index page.*/}
            {content}


        </div>
    );
}




