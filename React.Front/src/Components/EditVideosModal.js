import React, { Component } from 'react';
import { Modal, Button, Row, Col, Form, Container } from 'react-bootstrap'

export class EditVideosModal extends Component {
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
            try {
                fetch(process.env.REACT_APP_API + 'video/', {
                    method: 'PUT',
                    headers: {
                        'Accept': 'application/json',
                        'Authorization': 'Bearer ' + this.state.token,
                        'Content-Type': 'application/json'
                    },
                    body: JSON.stringify({
                        Id: event.target.Id.value,
                        Title: event.target.Title.value,
                        Description: event.target.Description.value,
                        IsPrivate: event.target.Private.value
                    })

                })
                    .then(res => {
                        if (res.status != 200) {
                            throw new Error(res.status);
                        }
                        resolve(res.json());

                    }

                    )

            }
            catch (e) {
                alert(e);
                resolve(e);
            }
        })

        return ret;
    }


    loader = async (event) => {
        const res = await this.handleSubmit(event);
        if (res) {
            alert(res);
            document.getElementById('editVideo').submit();
        } else {
            console.log('Undefined Behavior');
        }
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
                    <Form id="editVideo" onSubmit={this.loader}>
                    <Modal.Header closeButton>
                        <Modal.Title id="contained-modal-title-vcenter">
                            Edit Video
                        </Modal.Title>
                    </Modal.Header>
                    <Modal.Body>
                        <Container fluid="lg">
                        
                             <Row className="justify-content-md-center">
                                    <Form.Group controlId="Id">
                                        <Form.Control type="text" name="Id" required hidden
                                            disabled
                                            defaultValue={this.props.video.id}
                                            placeholder={this.props.video.id}>
                                        </Form.Control>
                                    </Form.Group>
                                    <Col>
                                    <Form.Group controlId="Title">
                                        <Form.Label>Title</Form.Label>
                                        <Form.Control type="text" name="Title" required
                                            defaultValue={this.props.video.title}
                                            placeholder={this.props.video.title}>
                                        </Form.Control>
                                    </Form.Group>
                                </Col>
                                </Row>
                            <Row className="mb-3">
                                
                                    <Form.Group controlId="Description">
                                        <Form.Label>Description</Form.Label>
                                        <Form.Control as="textarea" rows={3} type="Description" name="Description" required
                                            defaultValue={this.props.video.description}
                                            placeholder={this.props.video.description}>
                                        </Form.Control>
                                    </Form.Group>
                                <Col sm={3}>  
                                    <Form.Group controlId="Private">
                                        <Form.Label>Private</Form.Label>
                                        <Form.Select type="text" name="Private"

                                            defaultValue={this.props.video.isPrivate.toString()}

                                            placeholder={this.props.video.isPrivate.toString()}>
                                            <option>True</option>
                                            <option>False</option>

                                        </Form.Select>
                                    </Form.Group>
                                    <Form.Group>
                                       
                                    </Form.Group>
                                </Col>
                            </Row>
                                

                    </Container>

                    </Modal.Body>
                    <Modal.Footer>
                        
                        <Row xs="auto">
                        
                            <Col>
                                <Button variant="primary" type="submit">
                            Save
                                </Button>
                            </Col>
                            <Col>
                                <Button variant="danger" onClick={this.closeModal}>
                           Cancel
                            </Button>
                            </Col>
                        </Row>
                    
                    </Modal.Footer></Form>
                </Modal>
            </>
        )
    }


}