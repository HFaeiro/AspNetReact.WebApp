import React/*, { Component,useEffect }*/ from 'react';

import { Navigate } from 'react-router-dom';

export default function Logout(props) {


    //const location = useLocation();
    //    const { state } = location;

    //useEffect(() => {
    //    console.log(location);
    //    console.log(props);
    //}
    //   )

    //localStorage.setItem('token', '');
    //localStorage.setItem('loggedIn', 'false');
    return (
        <div>
            {props.logout()}

            <Navigate to={"/"} />


        </div>
    );
}




