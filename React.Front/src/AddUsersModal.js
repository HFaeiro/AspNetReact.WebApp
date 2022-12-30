import React, { Component } from 'react';
import { Modal, Button, Row, Col, Form } from 'react-bootstrap'
import { createPath } from 'react-router-dom';

export class AddUsersModal extends Component {
    constructor(props) {
        super(props);
        this.handleSubmit = this.handleSubmit.bind(this);
        this.state = {
            showModal: props.showModal,
            token: this.props.token
        };

    }
    openModal = () => this.setState({ showModal: true });
    closeModal = () => {
        this.setState({ showModal: false });
        if (this.props.dontShowButton)
            window.location = './login';
    }
    handleSubmit = async (event) => {
         const ret = await new Promise(resolve => {
            event.preventDefault();
            fetch(process.env.REACT_APP_API + 'users', {
                method: 'POST',
                headers: {
                    'Accept': 'application/json',
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify({
                    Username: event.target.Username.value,
                    Password: event.target.Password.value,
                    Privileges: event.target.Privileges.value
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
        const res = await this.handleSubmit(event);
        if (res) {
            if (res.status === 400)
                alert('Please Pick a Different Username!');
            else {

                alert('Username Created Successfully');
                this.closeModal();
            }
        }
        else {
            alert('Undefined Behavior');
        }
    }

    render() {
       
        return (
            <>
                <div className="d-flex align-items-center justify-content-center">
                {!this.props.dontShowButton ?  
                    <Button variant="primary" onClick={this.openModal}>
                        Create User
                    </Button>:
                    <></>
                    }
                

                </div>

                <Modal show={this.state.showModal}
                    onHide={this.closeModal}
                    size="lg"
                    aria-labelledby="contained-modal-title-vcenter"
                    centered>

                    <Modal.Header closeButton>
                        <Modal.Title id="contained-modal-title-vcenter">
                            Add User
                        </Modal.Title>
                    </Modal.Header>
                    <Modal.Body>
                        <Row>
                            <Col sm={6}>
                                <Form onSubmit={this.loader}>
                                    <Form.Group controlId="Username">
                                        <Form.Label>Username</Form.Label>
                                        <Form.Control type="text" name="Username" required
                                            placeholder="Username">
                                        </Form.Control>
                                    </Form.Group>
                                    <Form.Group controlId="Password">
                                        <Form.Label>Password</Form.Label>
                                        <Form.Control type="password" name="Password" required
                                            placeholder="Password">
                                        </Form.Control>
                                    </Form.Group>
                                    

                                    <Form.Group >
                                        <Form.Label>Privileges</Form.Label>
                                        <Form.Select type="text" name="Privileges" required
                                        >
                                            <option>None</option>
                                            <option>Team</option>
                                            <option>Mod</option>
                                            <option>Admin</option>
                                        </Form.Select>
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
            </>
        )
    }


} export default AddUsersModal;