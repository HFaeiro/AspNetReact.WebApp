import React, { Component } from 'react';
import { Modal, Button } from 'react-bootstrap'

export class MyModal extends Component {
    constructor(props) {
        super(props);


        this.state =
        {
            showModal: this.props.showModal,
            forceClose : false,
        }

    }
    //open and close modal lambas
    openModal = () => this.setState({ showModal: true });
    closeModal = () => {
        this.setState({ showModal: false });
        if (this.props.callback) {
            this.props.callback();
        }
    }


    render() {

        return (
            <>
                <Modal show={this.state.showModal}
                    onHide={this.closeModal}
                    size={this.props.size}
                    aria-labelledby="contained-modal-title-vcenter"
                    centered>
                    <Modal.Header closeButton>
                        <Modal.Title id="contained-modal-title-vcenter">
                            {this.props.modalHeaderTxt }
                        </Modal.Title>
                    </Modal.Header>
                    <Modal.Body>
                        {this.props.modalBodyTxt}
                        {this.props.forceClose ? <></> :
                             this.props.body 
                        }
                    </Modal.Body>
                    <Modal.Footer> 
                        {this.props.forceClose ? <></> :
                            this.props.footerButtons
                        }
                        <Button variant="danger" onClick={this.closeModal}>
                            Close
                        </Button>
                    </Modal.Footer>
                </Modal>
            </>
        )
    }


}