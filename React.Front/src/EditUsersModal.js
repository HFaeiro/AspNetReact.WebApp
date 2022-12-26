import React, { Component } from 'react';
import { Modal, Button, Row, Col, Form } from 'react-bootstrap'

export class EditUsersModal extends Component {
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
   async handleSubmit(event) {
   
        fetch(process.env.REACT_APP_API + 'users/' + event.target.Id.value , {
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
                
            
        }).then(res => res.json())
                .then((result) => {
                    this.closeModal()
                    alert(result);
                    

                },  
                (error) => {
                    alert('Failed:' + error);
                })

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
                                            defaultValue={this.props.uName }
                                        placeholder={this.props.uName }>
                                    </Form.Control>
                                    </Form.Group>
                                    <Form.Group controlId="Password">
                                        <Form.Label>Password</Form.Label>
                                        <Form.Control type="password" name="Password" required
                                            defaultValue={this.props.uPass}
                                            placeholder={this.props.uPass}>
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