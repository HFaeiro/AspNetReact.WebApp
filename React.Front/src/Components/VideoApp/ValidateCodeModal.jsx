import React, { Component } from 'react';
import { Button, Row, Col, Form } from 'react-bootstrap'
import { MyModal } from './Functions/MyModal';

export class ValidateCodeModal extends Component{
    constructor(props) {
        super(props);
        this.handleSubmit = this.handleSubmit.bind(this);
        this.requestNewCode = this.requestNewCode.bind(this);


        this.modalHeaderTxt = this.props.modalHeaderTxt ? this.props.modalHeaderTxt :  "Code Validation!";
        this.modalBodyTxt = this.props.modalBodyTxt ? this.props.modalBodyTxt :"Please enter the code that was sent to your email. If you don't see it, wait a few minutes and check your spam folder.";

       
        this.state =
        {
            showModal: true,
            token: this.props.token,
            modalHeaderTxt: this.modalHeaderTxt,
            modalBodyTxt: this.modalBodyTxt,
            forceClose : false,
        }

    }
    //sends a delete with id
    async handleSubmit(event) {
        event.preventDefault();
        fetch('/' + process.env.REACT_APP_API + 'authcode/', {
            method: 'POST',
            headers: {
                'Accept': 'application/json',
                'Authorization': 'Bearer ' + this.state.token,
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({
                UId: this.props.uId,
                Code: event.target.Code.value,

            })
        }).then(res => {
            let forceClose = false;
            if (res.status === 200) {
                forceClose = true;
                this.modalBodyTxt = "Code validation was a success. Please close this window and login!"
            }
            else {
                this.modalBodyTxt= "Sorry! You've entered the wrong code. Please try again or request a new code!";
            }
            this.setState(
                {
                    modalBodyTxt: this.modalBodyTxt,
                    forceClose: forceClose,
                }
            )

        }
        )

    }
    async requestNewCode() {
        fetch('/' + process.env.REACT_APP_API + 'authcode/', {
            method: 'PUT',
            headers: {
                'Accept': 'application/json',
                'Authorization': 'Bearer ' + this.state.token,
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({
                UId: this.props.uId,
                Code: "0"
            })
        }).then(res => {
            let forceClose = false;
            if (res.status === 200) {

            }
            else {
                this.modalBodyTxt = "Sorry! We are Unable to Provide you a new Code Right now, Please Try again later!";
                forceClose = true;
            }
            this.setState(
                {
                    modalBodyTxt: this.modalBodyTxt,
                    forceClose : forceClose,
                }
            )
        }
        )
    }
    render() {
        let body =            
                    <Row>
                        <Col sm={8}>
                            <Form onSubmit={this.handleSubmit}>
                                <Form.Group controlid="Code">
                                    <Form.Control type="text" name="Code" required
                                        placeholder={"Code"}>
                                    </Form.Control>
                                </Form.Group>
                                <Form.Group>
                                    <Button variant="primary" type="submit">
                                        Enter Code
                                    </Button>
                                </Form.Group>
                            </Form>

                        </Col>
                    </Row>


        return (
            <>
                <MyModal
                    showModal={this.state.showModal}
                    modalHeaderTxt={this.state.modalHeaderTxt}
                    modalBodyTxt={this.state.modalBodyTxt}
                    handleSubmit={this.handleSubmit}
                    forceClose={this.state.forceClose}
                    body={body}
                    footerButtons={<Button variant="primary" onClick={this.requestNewCode}>
                        Request New Code
                    </Button>}
                    callback={window.location = './login' }
                >
                </MyModal>
               
            </>
        )
    }


}