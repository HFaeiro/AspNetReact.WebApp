import React, { Component } from 'react';
import { Modal, Button, Row, Col, Form } from 'react-bootstrap'

export class EditUsersModal extends Component {
    state = {
        showModal: false,
        token: this.props.token
    };
    constructor(props) {
        super(props);
        this.loader = this.loader.bind(this);

    }
    openModal = () => this.setState({ showModal: true });
    closeModal = () => this.setState({ showModal: false });
    handleSubmit = async (event) => {
        const ret = await new Promise(resolve => {
            event.preventDefault();
            fetch('/' +process.env.REACT_APP_API + 'users/' + event.target.Id.value, {
                method: 'PUT',
                headers: {
                    'Accept': 'application/json',
                    'Authorization': 'Bearer ' + this.state.token,
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify({
                    UserId: event.target.Id.value,
                    Username: event.target.Username.value,
                    Password: event.target.Password.value,
                    Privileges: event.target.Privileges.value

                })

            })
                .then(res => {
                    if (res.status == 200) {
                        
                        if (this.props.myId == event.target.Id.value) {
                            var profile =
                            {
                                username: this.props.uName,
                                privileges: this.props.uPriv
                            }

                            var areEdits = false;
                            if (event.target.Username.value != '' && profile.Username != event.target.Username.value) {
                                profile.username = event.target.Username.value;
                                areEdits = true;
                            }

                            if (event.target.Privileges.value != '' && event.target.Privileges.value != profile.Privileges) {
                                profile.privileges = event.target.Privileges.value
                                areEdits = true;
                            }
                            if (areEdits == true)
                                this.props.updateProfile(profile);
                        }
                        event.target.Id.value = null;
                        event.target.Username.value = null;

                        event.target.Privileges.value = null;
                        document.getElementById('editUsers').submit();
                        
                    }
                    resolve(res.json())
            
                }

            )

        })
        return ret;
    }


    loader = async (event) => {
        const res = await this.handleSubmit(event);
        if (res) {
             alert(res);
        } else {
            console.log('Undefined Behavior');
        }
        event.target.Password.value = null;
    }

    render() {

        return (
            <>

                <Button variant="info" onClick={this.openModal}>
                    Edit
                </Button>



                <Modal show={this.state.showModal}
                    onHide={this.closeModal}
                    size="lg"
                    aria-labelledby="contained-modal-title-vcenter"
                    centered>

                    <Modal.Header closeButton>
                        <Modal.Title id="contained-modal-title-vcenter">
                            Edit User
                        </Modal.Title>
                    </Modal.Header>
                    <Modal.Body>
                        <Row>
                            <Col sm={6}>
                                <Form id="editUsers" onSubmit={this.loader}>
                                    <Form.Group controlId="Id">
                                        <Form.Label>Id</Form.Label>
                                        <Form.Control type="text" name="Id" required
                                            disabled
                                            defaultValue={this.props.uId}
                                            placeholder={this.props.uId}>
                                        </Form.Control>
                                    </Form.Group>
                                    <Form.Group controlId="Username">
                                        <Form.Label>Username</Form.Label>
                                        <Form.Control type="text" name="Username" required
                                            defaultValue={this.props.uName}
                                            placeholder={this.props.uName}>
                                        </Form.Control>
                                    </Form.Group>
                                    <Form.Group controlId="Password">
                                        <Form.Label>Password</Form.Label>
                                        <Form.Control type="password" name="Password" >

                                        </Form.Control>
                                    </Form.Group>

                                    <Form.Group controlId="Privileges">
                                        <Form.Label>Privileges</Form.Label>
                                        <Form.Select type="text" name="Privileges"

                                            defaultValue={this.props.uPriv}>

                                            <option>None</option>
                                            <option>Team</option>
                                            <option>Mod</option>
                                            <option>Admin</option>
                                        </Form.Select>
                                    </Form.Group>
                                    <Form.Group>
                                        <Button variant="primary" type="submit">
                                            Edit User
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


}