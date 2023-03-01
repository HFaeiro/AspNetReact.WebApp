import React, { Component } from 'react';
import { Form, Table } from 'react-bootstrap'
import { Navigate, Link } from 'react-router-dom';
import { useRef } from 'react';
import { MyVideos } from './MyVideos.js'
import './Home.css'
export class Home extends Component {
    state =
        {
            defaultUser: [],
            

        }
    componentDidMount() {
        this.handleSubmit();
    }


   

    async getDummyUserInfo() {
        fetch(process.env.REACT_APP_API + 'users/' + '1', {
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

                <MyVideos
                    profile={this.props.profile}
                    updateProfile={this.props.updateProfile}
                    incrementPollCount={this.props.incrementPollCount}
                    resetPollCount={this.props.resetPollCount}
                />
            </>
            :<> <div className="homePage" >
                <div><span>
                    You can either create a user or you can login using the default!
                </span>
                </div>
                <span>
                    Try this Username : {this.state.defaultUser.username}
                </span>
                <div> <span>
                    And this Password! : {this.state.defaultUser.password}
                </span>
                </div>
                <div> <span>
                    Might see the password change right here if you edit it
                </span>
                </div>

            </div><div className="loginButton">
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