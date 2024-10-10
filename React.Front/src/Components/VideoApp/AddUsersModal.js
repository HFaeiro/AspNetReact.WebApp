import React, { Component } from 'react';
import { Modal, Button, Row, Col, Form } from 'react-bootstrap'
import {ValidateCodeModal }  from './ValidateCodeModal'

export class AddUsersModal extends Component {
    constructor(props) {
        super(props);
        this.handleSubmit = this.handleSubmit.bind(this);
        this.state = {
            showModal: props.showModal,
            token: this.props.token,
            showCodeValidation: false,
            uId : undefined,
        };

    }
    openModal = () => this.setState({ showModal: true });
    closeModal = () => {
        this.setState({ showModal: false });
        if (this.props.dontShowButton) {
            window.location = './login';
        }        
    }
    handleSubmit = async (event) => {
         const ret = await new Promise(resolve => {
            event.preventDefault();
             fetch('/' +process.env.REACT_APP_API + 'users', {
                method: 'POST',
                headers: {
                    'Accept': 'application/json',
                    'Content-Type': 'application/json'
                },
                 body: JSON.stringify({

                    Email: event.target.Email.value,
                    Username: event.target.Username.value,
                    Password: event.target.Password.value
                })


            }).then(res => res.json())
                .then(data => {
                    console.log(data)
                    resolve(data);

                })
        })
        return ret;
    }

    loader = async (event) => {

        event.preventDefault();
        if (event.target.Password.value !== event.target.Password2.value) {
            alert('Passwords dont match!');
            return;
        }

        const res = await this.handleSubmit(event);
        if (res) {
            if (res.status === 400)
                alert('Please Pick a Different Username or Email!');
            else {
                this.userInfo = res;
                alert('User Created Successfully');

                event.target.Email.value = null;
                event.target.Username.value = null;
                event.target.Password.value = null;
                //document.getElementById('addUsers').submit();
                this.setState(
                    {
                        showCodeValidation: true,
                        uId: this.userInfo.userId,
                    }
                )
            }
        }
        else {
           console.log('Undefined Behavior');
        }

    }

    render() {
       
        return (
            <>
                <div className="addUsersModal">
                {!this.props.dontShowButton ?  
                    <Button variant="primary" onClick={this.openModal}>
                        Sign Up!
                    </Button>:
                    <></>
                    }                

                </div>
                {this.state.showCodeValidation && this.state.uId
                    ? <ValidateCodeModal
                        uId={this.state.uId}
                /> :
                <Modal show={this.state.showModal}
                    onHide={this.closeModal}
                    size="lg"
                    aria-labelledby="contained-modal-title-vcenter"
                    centered>

                    <Modal.Header closeButton>
                        <Modal.Title id="contained-modal-title-vcenter">
                           Welcome! Please fill out the form below to sign up!
                        </Modal.Title>
                    </Modal.Header>
                    <Modal.Body>
                        <Row>
                            <Col sm={6}>
                                <Form id="addUsers" onSubmit={this.loader}>
                                    <Form.Group controlid="Email">
                                        <Form.Label>Email</Form.Label>
                                        <Form.Control type="text" name="Email" required
                                            placeholder="Email">
                                        </Form.Control>
                                    </Form.Group>
                                    <Form.Group controlid="Username">
                                        <Form.Label>Username</Form.Label>
                                        <Form.Control type="text" name="Username" required
                                            placeholder="Username">
                                        </Form.Control>
                                    </Form.Group>
                                    <Form.Group controlid="Password">
                                        <Form.Label>Password</Form.Label>
                                        <Form.Control type="password" name="Password" required
                                            placeholder="Password">
                                        </Form.Control>
                                    </Form.Group>

                                     <Form.Group controlid="Password2">
                                        <Form.Label>Password</Form.Label>
                                            <Form.Control type="Password" name="Password2" required
                                        placeholder="Password">
                                        </Form.Control>
                                     </Form.Group>

                                    <Form.Group>
                                        <Button variant="primary" type="submit">
                                            Add User
                                        </Button>
                                    </Form.Group>
                                </Form>

                            </Col>
                        </Row>

                    </Modal.Body>
                    <Modal.Footer>
                        <Button variant="danger" onClick={this.closeModal}>
                            Close
                        </Button>
                        
                    </Modal.Footer>
                </Modal>
                }
            </>
        )
    }


} export default AddUsersModal;