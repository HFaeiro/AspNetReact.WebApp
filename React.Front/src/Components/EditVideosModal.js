import React, { Component } from 'react';
import { Modal, Button, Row, Col, Form } from 'react-bootstrap'

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

                    <Modal.Header closeButton>
                        <Modal.Title id="contained-modal-title-vcenter">
                            Edit Video
                        </Modal.Title>
                    </Modal.Header>
                    <Modal.Body>
                        <Row>
                            <Col sm={6}>
                                <Form id="editVideo" onSubmit={this.loader}>
                                    <Form.Group controlId="Id">
                                        <Form.Control type="text" name="Id" required hidden
                                            disabled
                                            defaultValue={this.props.video.id}
                                            placeholder={this.props.video.id}>
                                        </Form.Control>
                                    </Form.Group>
                                    <Form.Group controlId="Title">
                                        <Form.Label>Title</Form.Label>
                                        <Form.Control type="text" name="Title" required
                                            defaultValue={this.props.video.title}
                                            placeholder={this.props.video.title}>
                                        </Form.Control>
                                    </Form.Group>
                                    <Form.Group controlId="Description">
                                        <Form.Label>Description</Form.Label>
                                        <Form.Control type="Description" name="Description" required
                                            defaultValue={this.props.video.description}
                                            placeholder={this.props.video.description}>
                                        </Form.Control>
                                    </Form.Group>

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
                                        <Button variant="primary" type="submit">
                                            Save
                                        </Button>
                                    </Form.Group>
                                </Form>

                            </Col>
                        </Row>

                    </Modal.Body>
                    <Modal.Footer>
                        <Button variant="danger" onClick={this.closeModal}>
                            Done
                        </Button>
                    </Modal.Footer>
                </Modal>
            </>
        )
    }


}