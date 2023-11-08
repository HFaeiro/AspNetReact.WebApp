import React, { Component } from 'react';
import { Modal, Button, Row, Col, Form } from 'react-bootstrap'
import { EditUsersModal } from './EditUsersModal';

export class DeleteUsersModal extends Component {
    state = {
        showModal: false,
        token : this.props.token
    };
    constructor(props) {
        super(props);
        this.handleSubmit = this.handleSubmit.bind(this);

    }
    //open and close modal lambas
    openModal = () => this.setState({ showModal: true });
    closeModal = () => this.setState({ showModal: false });

    //sends a delete with id
    async handleSubmit(event) {
        fetch('/' +process.env.REACT_APP_API + 'users/' + event.target.Id.value, {
            method: 'DELETE',
            headers: {
                'Accept': 'application/json',
                'Authorization': 'Bearer ' + this.state.token,
                'Content-Type': 'application/json'
            }

        })
           
    }



    render() {

        return (
            <>
                <Button variant="danger" onClick={this.openModal}>
                    Delete
                </Button>



                <Modal show={this.state.showModal}
                    onHide={this.closeModal}
                    size="sm"
                    aria-labelledby="contained-modal-title-vcenter"
                    centered>

                    <Modal.Header closeButton>
                        <Modal.Title id="contained-modal-title-vcenter">
                            Delete User
                        </Modal.Title>
                    </Modal.Header>
                    <Modal.Body>
                        Are you sure you want do delete this user?
                        <Row>
                            <Col sm={8}>
                                <Form onSubmit={this.handleSubmit}>
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
                                            disabled
                                            defaultValue={this.props.uName}
                                            placeholder={this.props.uName}>
                                        </Form.Control>
                                    </Form.Group>
                                    <Form.Group controlId="Privileges">
                                        <Form.Label>Privileges</Form.Label>
                                        <Form.Control type="text" name="Privileges"
                                            disabled

                                            defaultValue={this.props.uPriv}
                                            placeholder={this.props.uPriv}>
                                        </Form.Control>
                                    </Form.Group>
                                    <Form.Group>
                                        <Button variant="danger" type="submit">
                                            Delete User
                                        </Button>
                                    </Form.Group>
                                </Form>

                            </Col>
                        </Row>

                    </Modal.Body>
                    <Modal.Footer>
                        <EditUsersModal
                            uId={this.props.uId}
                            uName={this.props.uName}
                            uPass={this.props.uPass}
                            uPriv={this.props.uPriv}
                        />
                        <Button variant="primary" onClick={this.closeModal}>
                            Close
                        </Button>

                    </Modal.Footer>
                </Modal>
            </>
        )
    }


}