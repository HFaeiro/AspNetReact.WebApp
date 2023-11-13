import React, { Component } from 'react';
import { Modal, Button, Row, Col, Form, Container, FloatingLabel } from 'react-bootstrap'

export class EditVideosModal extends Component {
    state = {        
        showModal: (this.props.showModal ? this.props.showModal : false),
        token: this.props.token,
        video: this.props.video
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
                if (!this.props.taskId) {
                    fetch('/' + process.env.REACT_APP_API + 'video/', {
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
                else {                    
                    this.props.editParent(event.target);
                    resolve(false);
                }
            }
            catch (e) {                
                resolve(e);
            }
        })

        return ret;
    }


    loader = async (event) => {
        const res = await this.handleSubmit(event);
        if (res) {            
            document.getElementById('editVideo').submit();
        } else {
            //we no longer have undefined behavior here ;)
            this.setState(
                {
                    showModal: false
                }
            )
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
                                Edit Your Video
                            </Modal.Title>
                        </Modal.Header>
                        <Modal.Body>
                            <Container fluid="lg">

                                <Row className="mb-3">                                    
                                    <Col>
                                        <Form.Group controlId="Title">
                                            <FloatingLabel controlId="floatingInput" label="Video Title"
                                                className="mb-3">
                                                <Form.Control type="text" name="Title" required
                                                    defaultValue={this.props.video.title}
                                                    placeholder={this.props.video.title}>
                                                </Form.Control>
                                            </FloatingLabel>
                                        </Form.Group>

                                    </Col>
                                    <Form.Group controlId="Description">
                                        <FloatingLabel controlId="floatingInput" label="Description">
                                            <Form.Control as="textarea" rows={3} type="Description" name="Description" required
                                                defaultValue={this.props.video.description}
                                                placeholder={this.props.video.description}>
                                            </Form.Control>
                                        </FloatingLabel>
                                    </Form.Group>
                                    <Col sm={3}>

                                        <Form.Group controlId="Private">
                                            <FloatingLabel controlID="floatingInput" label="Privacy">

                                                <Form.Select type="text" name="Private"

                            
                                                    defaultValue={this.props.video.isPrivate? this.props.video.isPrivate.toString() : "True"}

                                                    placeholder={"True"}>
                                                    <option value="True">Private</option>
                                                    <option value="False">Public</option>

                                                </Form.Select>
                                            </FloatingLabel>
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