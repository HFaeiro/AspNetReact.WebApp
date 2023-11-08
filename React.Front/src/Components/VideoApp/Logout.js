import React/*, { Component,useEffect }*/ from 'react';
import { Button, Modal } from 'react-bootstrap';
import { Navigate } from 'react-router-dom';

export default function Logout(props) {
    const closeModal = async () => {
        const res = await props.setStateAsync({ logoutModal: false });
        window.location = './';
    }
    //call logout in app.js or
    //navigate to the index page.
    let content = props.isLoggedIn === 'true' || !props.showModal ?
        <Modal show={props.showModal}
            onHide={closeModal}
            size="sm"
            aria-labelledby="contained-modal-title-vcenter"
            centered>

            <Modal.Header closeButton>
                <Modal.Title id="contained-modal-title-vcenter">
                   Logout?!
                </Modal.Title>
            </Modal.Header>
            <Modal.Body>
                <Button variant='danger' onClick={() => props.logout()}>Yes!</Button>
                <Button variant='primary' onClick={() => closeModal() }>Nevermind!</Button>
            </Modal.Body>
            </Modal>
        : <Navigate to={"/videoapp"} />


    return (

        <div>
            {content}

        </div>
    );
}




