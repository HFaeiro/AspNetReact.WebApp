import React, { Component } from 'react';
import { Form, Table } from 'react-bootstrap'
import { Link } from 'react-router-dom';
import { useRef } from 'react';
import { MyVideos } from './MyVideos.js'

import { AddUsersModal } from './AddUsersModal'
import './Home.css'
export class Home extends Component {
    constructor(props) {
        super(props);
        this.onAddUserClose = this.onAddUserClose.bind(this);
        this.openAddUserModal = this.openAddUserModal.bind(this);
        this.state =
        {
            defaultUser: [],
            showAddUserModal: false

        }
    }
    componentDidMount() {
        this.handleSubmit();
    }

    async onAddUserClose() {

    }
    async openAddUserModal() {
        this.setState({ showAddUserModal: true });
    }

    async getDummyUserInfo() {
        fetch('/' +process.env.REACT_APP_API + 'users/' + '1', {
            method: 'Get',
            headers: {
                'Accept': 'application/json',
                'Content-Type': 'application/json'
            }

        }).then(res => res.json())
            .then((result) => {
                this.setState({ defaultUser: result });


            },
                (error) => {
                    console.log('Failed:' + error);
                })
    }



    async handleSubmit() {
        if (this.props.profile.username == '')
        await this.getDummyUserInfo();
        
    }



    render() {

        
        //let contents = username if it exists.
        let contents = this.props.profile.username ?
            <>

                {/*<MyVideos
                    profile={this.props.profile}
                    updateProfile={this.props.updateProfile}
                    incrementPollCount={this.props.incrementPollCount}
                    resetPollCount={this.props.resetPollCount}
                />*/}
            </>
            : <> <div className="homePage" >

            <div className="createAccountButton">
                    <AddUsersModal
                        showModal={false}
                        dontShowButton={false}
                    /> </div>
               </div>
                <div className="loginButton">
           <Link to="login" className="btn btn-primary" >
                    Login!
                    </Link></div></>


        return (
            <div className="mt-5 justify-content-left">

                {contents}
                

            </div>
        );
    }


}