import React, { Component } from 'react';
import { Modal, Button, Row, Col, Form } from 'react-bootstrap'

export class AddUsersModal extends Component {
    state = {
        showModal: false,
        token : this.props.token
    };
    constructor(props) {
        super(props);
        this.handleSubmit = this.handleSubmit.bind(this);

    }
    openModal = () => this.setState({ showModal: true });
    closeModal = () => this.setState({ showModal: false });
    handleSubmit = async (event) => {
         const ret = await new Promise(resolve => {
            event.preventDefault();
            fetch(process.env.REACT_APP_API + 'users', {
                method: 'POST',
                headers: {
                    'Accept': 'application/json',
                    'Authorization': 'Bearer ' + this.state.token,
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify({
                    Username: event.target.Username.value,
                    Password: event.target.Password.value,
                    Privileges: event.target.Privileges.value
                })


            }).then(res => res.json())
                .then((result) => {
                    resolve(result);
                    
                },
                    (error) => {
                        //alert('Failed error = ' + error);
                        resolve(error);
                    })
        })
        return ret;
    }

    loader = async (event) => {
        const res = await this.handleSubmit(event);
        if (res) {

            alert(res);
        }
        else {
            alert('no resolve');
        }
    }

    render() {
       
        return (
            <>
                <div className="d-flex align-items-center justify-content-center">
                    <Button variant="primary" onClick={this.openModal}>
                        Create User
                    </Button>

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